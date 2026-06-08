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
        private EmployeesPanel _employeesPanel;
        private ComputersPanel _computersPanel;

        private readonly Color _activeBg = Color.FromArgb(100, 116, 139);
        private readonly Color _hoverBg = Color.FromArgb(100, 116, 139);
        private readonly Color _defaultBg = Color.Transparent;

        private Employee _loggedInEmployee;



        public Form1(Employee loggedInEmployee)
        {
            InitializeComponent();
            _loggedInEmployee = loggedInEmployee;

            if (_loggedInEmployee?.Role == "admin")
            {
                _computersPanel = new ComputersPanel();
                _computersPanel.Visible = false;
                main.Controls.Add(_computersPanel, 0, 0);
                main.SetRowSpan(_computersPanel, 2);
                
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            UiHelper.MakeRounded(DashboardBTN, 20);
            UiHelper.MakeRounded(membersBTN, 20);
            UiHelper.MakeRounded(LogOutBTN, 20);
            UiHelper.MakeRounded(ComputersBTN, 20);
            WireNavButton(DashboardBTN);
            WireNavButton(membersBTN);
            WireNavButton(LogOutBTN);
            WireNavButton(ComputersBTN);

            // ✅ Only show Employee button if admin
            if (_loggedInEmployee?.Role == "admin")
            {
                EmployeeBTN.Visible = true;
                UiHelper.MakeRounded(EmployeeBTN, 20);
                WireNavButton(EmployeeBTN);
            }
            else
            {
                EmployeeBTN.Visible = false;
            }

            if (_loggedInEmployee?.Role == "admin")
            {
                _computersPanel = new ComputersPanel();
                _computersPanel.Visible = false; // Starts hidden
                main.Controls.Add(_computersPanel, 0, 0);
                main.SetRowSpan(_computersPanel, 2);
            }

            UiHelper.AddRightBorder(left_navigation,
                color: Color.FromArgb(0, 122, 204), thickness: 2);

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
                                    string limitNotes = modal.SelectedHours > 0 ? $"LIMIT:{modal.SelectedHours}" : "OPEN";

                                    switch (modal.SelectedAction)
                                    {
                                        case SessionModal.Action.Start:
                                            await SupabaseService.StartSessionAsync(pc.Id, modal.CustomerName);
                                            await SupabaseService.UpdateComputerNotesAsync(pc.Id, limitNotes);
                                            if (modal.SelectedMemberId != null)
                                            {
                                                await SupabaseService.StartMemberSessionAsync(modal.SelectedMemberId, pc.Id);
                                                await SupabaseService.UpdateMemberBalanceAsync(modal.SelectedMemberId, 0);
                                            }
                                            break;

                                        case SessionModal.Action.Reserve:
                                            await SupabaseService.ReserveAsync(pc.Id, modal.CustomerName);
                                            await SupabaseService.UpdateComputerNotesAsync(pc.Id, limitNotes);
                                            break;

                                        case SessionModal.Action.ExtendOnly:
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
                                            await PauseSessionWithMemberSave(pc);
                                            break;

                                        case SessionModal.Action.End:
                                            await EndSessionWithMemberSave(pc);
                                            break;

                                        case SessionModal.Action.Free:
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

            // ✅ Pass _loggedInEmployee into MembersPanel
            _membersPanel = new MembersPanel(_loggedInEmployee);
            _membersPanel.Visible = false;
            main.Controls.Add(_membersPanel, 0, 0);
            main.SetRowSpan(_membersPanel, 2);

            // ✅ Only create EmployeesPanel for admins
            if (_loggedInEmployee?.Role == "admin")
            {
                _employeesPanel = new EmployeesPanel();
                _employeesPanel.Visible = false;
                main.Controls.Add(_employeesPanel, 0, 0);
                main.SetRowSpan(_employeesPanel, 2);
            }

            SetActive(DashboardBTN);
            await RefreshDashboardAsync();
        }

        // ── End session + save remaining time to member ───────────────────────
        private async Task EndSessionWithMemberSave(Computer pc)
        {
            var members = await SupabaseService.GetMembersAsync();
            var member = members.Find(m => m.FullName == pc.CurrentCustomer);

            if (member != null && pc.SessionStart.HasValue)
            {
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

        // ── Pause: save remaining time to member, free computer ───────────────
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
                            secondsRemainingTotal = (int)(unspentHoursLeft * 3600);
                    }
                }

                await SupabaseService.UpdateMemberBalanceAsync(member.Id, secondsRemainingTotal);

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
            // Only include EmployeeBTN in reset loop if admin
            var buttons = _loggedInEmployee?.Role == "admin"
                ? new Button[] { DashboardBTN, membersBTN, EmployeeBTN }
                : new Button[] { DashboardBTN, membersBTN };

            foreach (Button b in buttons)
            {
                b.BackColor = _defaultBg;
                b.ForeColor = Color.White;
            }
            btn.BackColor = _activeBg;
            btn.ForeColor = Color.White;
            _activeBtn = btn;
        }

        // ── Button clicks ─────────────────────────────────────────────────────
        private void DashboardBTN_Click(object sender, EventArgs e)
        {
            SetActive(DashboardBTN);
            _dashboardPanel.Visible = true;
            _membersPanel.Visible = false;
            if (_employeesPanel != null) _employeesPanel.Visible = false;
            if (_computersPanel != null) _computersPanel.Visible = false;
        }

        private async void membersBTN_Click(object sender, EventArgs e)
        {
            SetActive(membersBTN);
            _dashboardPanel.Visible = false;
            _membersPanel.Visible = true;
            if (_employeesPanel != null) _employeesPanel.Visible = false;
            if (_computersPanel != null) _computersPanel.Visible = false;
            await _membersPanel.LoadMembersAsync();
        }

        private async void EmployeeBTN_Click(object sender, EventArgs e)
        {
            if (_loggedInEmployee?.Role != "admin") return; // safety guard
            SetActive(EmployeeBTN);
            _dashboardPanel.Visible = false;
            _membersPanel.Visible = false;
            if (_computersPanel != null) _computersPanel.Visible = false;
            if (_employeesPanel != null)
            {
                _employeesPanel.Visible = true;
                await _employeesPanel.LoadEmployeesAsync();
            }
        }

        private void LogOutBTN_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Log Out",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_dashboardPanel != null)
                    _dashboardPanel.StopLiveUpdates();

                var loginForm = new LoginForm();
                loginForm.Show();
                this.Hide();
                this.Close();
            }
        }

        private void body_Paint(object sender, PaintEventArgs e) { }

        private async void computersBTN_ClickAsync(object sender, EventArgs e)
        {
            if (_loggedInEmployee?.Role != "admin") return;
            SetActive(ComputersBTN);
            _dashboardPanel.Visible = false;
            _membersPanel.Visible = false;
            _employeesPanel.Visible = false;
            _computersPanel.Visible = true;
            await _computersPanel.LoadComputersAsync();
        }

        private void main_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}