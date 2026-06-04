using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    public partial class Form1 : Form
    {
        private Button _activeBtn = null;
        private DashboardPanel _dashboardPanel = null;
        private MembersPanel _membersPanel = null;

        private readonly Color _activeBg = Color.FromArgb(100, 116, 139);
        private readonly Color _hoverBg = Color.FromArgb(100, 116, 139);
        private readonly Color _defaultBg = Color.Transparent;
        private EmployeesPanel _employeesPanel;
        private Member _currentUser;
        private Employee _loggedInEmployee;

        public Form1(Employee loggedInEmployee)
        {
            InitializeComponent();
            _loggedInEmployee = loggedInEmployee;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            UiHelper.MakeRounded(DashboardBTN, 20);
            UiHelper.MakeRounded(membersBTN, 20);
            UiHelper.MakeRounded(EmployeeBTN, 20);
            UiHelper.MakeRounded(LogOutBTN, 20);
            UiHelper.AddRightBorder(left_navigation,
                color: Color.FromArgb(0, 122, 204), thickness: 2);

            WireNavButton(DashboardBTN);
            WireNavButton(membersBTN);
            WireNavButton(EmployeeBTN);
            WireNavButton(LogOutBTN);

            _dashboardPanel = new DashboardPanel();

            _dashboardPanel.ComputerCardClicked += (pc) =>
            {
                _dashboardPanel.StopLiveUpdates();

                this.BeginInvoke((Action)(() =>
                {
                    using (var modal = new SessionModal(pc))
                    {
                        modal.StartPosition = FormStartPosition.CenterParent;
                        var result = modal.ShowDialog(this);

                        if (result == DialogResult.OK)
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    // Generate the custom string label to save how much time they requested
                                    string limitNotes = modal.SelectedHours > 0 ? $"LIMIT:{modal.SelectedHours}" : "OPEN";

                                    switch (modal.SelectedAction)
                                    {
                                        case SessionModal.Action.Start:
                                            // Start active session & record the time limitation constraint
                                            await SupabaseService.StartSessionAsync(pc.Id, modal.CustomerName);
                                            await SupabaseService.UpdateComputerNotesAsync(pc.Id, limitNotes);

                                            if (modal.SelectedMemberId != null)
                                            {
                                                await SupabaseService.StartMemberSessionAsync(modal.SelectedMemberId, pc.Id);
                                                // Deduct/clear their saved balance wallet now that it is running live on the PC
                                                await SupabaseService.UpdateMemberBalanceAsync(modal.SelectedMemberId, 0);
                                            }
                                            break;

                                        case SessionModal.Action.Reserve:
                                            // Lock computer status to 'reserved' and hold it with their timeframe target
                                            await SupabaseService.ReserveAsync(pc.Id, modal.CustomerName);
                                            await SupabaseService.UpdateComputerNotesAsync(pc.Id, limitNotes);
                                            break;

                                        case SessionModal.Action.ExtendOnly:
                                            // EXTEND TIME: Pull current limit value, increment it, and append it back to Supabase notes
                                            if (!string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
                                            {
                                                if (double.TryParse(pc.Notes.Replace("LIMIT:", ""), out double activeLimit))
                                                {
                                                    double newLimit = activeLimit + modal.ExtendedHours;
                                                    await SupabaseService.UpdateComputerNotesAsync(pc.Id, $"LIMIT:{newLimit}");
                                                    MessageBox.Show($"Successfully extended user session by +{modal.ExtendedHours} hours!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                            }
                                            break;

                                        case SessionModal.Action.StopTime:
                                            // STOP TIME: Pause session context, save what is left to their wallet, and free the PC
                                            await PauseSessionWithMemberSave(pc);
                                            break;

                                        case SessionModal.Action.End:
                                            // END SESSION: Terminate session explicitly and capture any remaining balance tracking offsets
                                            await EndSessionWithMemberSave(pc);
                                            break;

                                        case SessionModal.Action.Free:
                                            // Clear out the reservation details and wipe the timer constraints string clean
                                            await SupabaseService.FreeComputerAsync(pc.Id);
                                            await SupabaseService.UpdateComputerNotesAsync(pc.Id, "");
                                            break;
                                    }

                                    this.BeginInvoke((Action)(async () =>
                                    {
                                        await RefreshDashboardAsync();
                                        _dashboardPanel.StartLiveUpdates();
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        MessageBox.Show("Database update error: " + ex.Message, "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        _dashboardPanel.StartLiveUpdates();
                                    }));
                                }
                            });
                        }
                        else
                        {
                            _dashboardPanel.StartLiveUpdates();
                        }
                    }
                }));
            };

            main.Controls.Add(_dashboardPanel, 0, 0);
            main.SetRowSpan(_dashboardPanel, 2);

            _membersPanel = new MembersPanel();
            _membersPanel.Visible = false;
            main.Controls.Add(_membersPanel, 0, 0);
            main.SetRowSpan(_membersPanel, 2);

            _employeesPanel = new EmployeesPanel();
            _employeesPanel.Visible = false;
            main.Controls.Add(_employeesPanel, 0, 0);
            main.SetRowSpan(_employeesPanel, 2);

            SetActive(DashboardBTN);
            await RefreshDashboardAsync();
        }

        // ── End session + save time to member if one was using it ─────────────
        private async Task EndSessionWithMemberSave(Computer pc)
        {
            // Find the computer's active customer in the members list to save their time
            var members = await SupabaseService.GetMembersAsync();
            var member = members.Find(m => m.FullName == pc.CurrentCustomer);

            if (member != null && pc.SessionStart.HasValue)
            {
                // Calculate unspent time based on notes limit
                if (!string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
                {
                    if (double.TryParse(pc.Notes.Replace("LIMIT:", ""), out double totalAllowedHours))
                    {
                        TimeSpan elapsed = DateTime.UtcNow - pc.SessionStart.Value;
                        double unspentHoursLeft = totalAllowedHours - elapsed.TotalHours;

                        if (unspentHoursLeft > 0.01)
                        {
                            int secondsRemainingTotal = (int)(unspentHoursLeft * 3600);
                            await SupabaseService.UpdateMemberBalanceAsync(member.Id, secondsRemainingTotal);
                        }
                    }
                }
            }

            await SupabaseService.FreeComputerAsync(pc.Id);
            await SupabaseService.UpdateComputerNotesAsync(pc.Id, "");
        }

        // ── Pause: save time to member account, free the computer ─────────────
        private async Task PauseSessionWithMemberSave(Computer pc)
        {
            var members = await SupabaseService.GetMembersAsync();
            var member = members.Find(m => m.FullName == pc.CurrentCustomer);

            if (member != null && pc.SessionStart.HasValue)
            {
                int secondsRemainingTotal = 0;

                if (!string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
                {
                    if (double.TryParse(pc.Notes.Replace("LIMIT:", ""), out double totalAllowedHours))
                    {
                        TimeSpan elapsed = DateTime.UtcNow - pc.SessionStart.Value;
                        double unspentHoursLeft = totalAllowedHours - elapsed.TotalHours;
                        if (unspentHoursLeft > 0)
                        {
                            secondsRemainingTotal = (int)(unspentHoursLeft * 3600);
                        }
                    }
                }

                // Save leftover time tokens to the member's wallet balance
                await SupabaseService.UpdateMemberBalanceAsync(member.Id, secondsRemainingTotal);

                // Format into a human-readable string display
                var h = secondsRemainingTotal / 3600;
                var m = (secondsRemainingTotal % 3600) / 60;
                string readableTime = h > 0 ? $"{h}h {m}m" : $"{m}m";

                MessageBox.Show(
                    $"Session stopped early!\nTime balance saved to {pc.CurrentCustomer}'s account.\n" +
                    $"Total saved remaining balance: {readableTime}",
                    "Time Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "Session ended (guest — no profile balance adjustments applied).",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            await SupabaseService.FreeComputerAsync(pc.Id);
            await SupabaseService.UpdateComputerNotesAsync(pc.Id, "");
        }

        // ── Fetch computers ───────────────────────────────────────────────────
        private async Task RefreshDashboardAsync()
        {
            try
            {
                List<Computer> computers = await SupabaseService.GetComputersAsync();
                _dashboardPanel.LoadComputers(computers);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load computers:\n" + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Nav helpers ───────────────────────────────────────────────────────
        private void WireNavButton(Button btn)
        {
            btn.MouseEnter += (s, e) =>
            {
                if (btn != _activeBtn) btn.BackColor = _hoverBg;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != _activeBtn) btn.BackColor = _defaultBg;
            };
        }

        private void SetActive(Button btn)
        {
            // Update the array to include EmployeeBTN
            foreach (Button b in new Button[] { DashboardBTN, membersBTN, EmployeeBTN })
            {
                b.BackColor = _defaultBg;
                b.ForeColor = Color.White;
            }

            // Set the clicked button to active
            btn.BackColor = _activeBg;
            btn.ForeColor = Color.White;
            _activeBtn = btn;
        }

        // ── Button clicks ─────────────────────────────────────────────────────
        private void DashboardBTN_Click(object sender, EventArgs e)
        {
            SetActive(DashboardBTN);

            // Explicitly hide others
            _dashboardPanel.Visible = true;
            _membersPanel.Visible = false;
            _employeesPanel.Visible = false;
        }

        private async void membersBTN_Click(object sender, EventArgs e)
        {
            SetActive(membersBTN);

            // Explicitly hide others
            _dashboardPanel.Visible = false;
            _membersPanel.Visible = true;
            _employeesPanel.Visible = false;

            await _membersPanel.LoadMembersAsync();
        }

        private void body_Paint(object sender, PaintEventArgs e) { }

        private async void EmployeeBTN_Click(object sender, EventArgs e)
        {
            SetActive(EmployeeBTN);

            // Explicitly hide others
            _dashboardPanel.Visible = false;
            _membersPanel.Visible = false;
            _employeesPanel.Visible = true;

            await _employeesPanel.LoadEmployeesAsync();
        }

        private void LogOutBTN_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Log Out",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 2. Stop live updates so you don't get background errors while closing
                if (_dashboardPanel != null)
                {
                    _dashboardPanel.StopLiveUpdates();
                }

                // 3. Show the login form
                // Replace 'LoginForm' with the exact class name of your login screen
                var loginForm = new LoginForm();
                loginForm.Show();

                // 4. Close this form
                this.Hide(); // Hide first to prevent flickering
                this.Close();
            }
        }
    }
}