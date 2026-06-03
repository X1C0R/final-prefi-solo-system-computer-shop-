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

        private readonly Color _activeBg = Color.FromArgb(100, 116, 139);
        private readonly Color _hoverBg = Color.FromArgb(100, 116, 139);
        private readonly Color _defaultBg = Color.Transparent;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Rounded nav buttons + sidebar border
            UiHelper.MakeRounded(DashboardBTN, 20);
            UiHelper.MakeRounded(membersBTN, 20);
            UiHelper.AddRightBorder(left_navigation,
                color: Color.FromArgb(0, 122, 204), thickness: 2);

            // Hover effects
            WireNavButton(DashboardBTN);
            WireNavButton(membersBTN);

            // Build dashboard panel
            _dashboardPanel = new DashboardPanel();

            _dashboardPanel.ComputerCardClicked += async (pc) =>
            {
                // ✅ C# 7.3: use regular using block, not using declaration
                using (var modal = new SessionModal(pc))
                {
                    var result = modal.ShowDialog(this);

                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            switch (modal.SelectedAction)
                            {
                                case SessionModal.Action.Start:
                                    await SupabaseService.StartSessionAsync(pc.Id, modal.CustomerName);
                                    break;
                                case SessionModal.Action.End:
                                    await SupabaseService.EndSessionAsync(pc);
                                    break;
                                case SessionModal.Action.Reserve:
                                    await SupabaseService.ReserveAsync(pc.Id, modal.CustomerName);
                                    break;
                                case SessionModal.Action.Free:
                                    await SupabaseService.FreeComputerAsync(pc.Id);
                                    break;
                            }

                            await RefreshDashboardAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

  
            main.Controls.Add(_dashboardPanel, 0, 0);
            main.SetRowSpan(_dashboardPanel, 2);

      
            SetActive(DashboardBTN);
            await RefreshDashboardAsync();
        }

        // ── Fetch computers and push to dashboard ─────────────────────────────
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
                if (btn != _activeBtn)
                    btn.BackColor = _hoverBg;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != _activeBtn)
                    btn.BackColor = _defaultBg;
            };
        }

        private void SetActive(Button btn)
        {
            foreach (Button b in new Button[] { DashboardBTN, membersBTN })
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
        }

        private void membersBTN_Click(object sender, EventArgs e)
        {
            SetActive(membersBTN);
            _dashboardPanel.Visible = false;
            // show members panel here when ready
        }

        private void body_Paint(object sender, PaintEventArgs e) { }
    }
}