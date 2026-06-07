using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace computerShop
{
    public partial class SessionModal : Form
    {
        public enum Action { Start, End, Pause, Reserve, Free, StopTime, ExtendOnly }
        public Action SelectedAction { get; private set; }
        public string CustomerName => nameInput?.Text?.Trim() ?? "";
        public string SelectedMemberId { get; private set; }

        // Returns the calculated fractional hours limit from the user's input
        public double SelectedHours { get; private set; } = -1;
        public double ExtendedHours { get; private set; } = 0; // Used purely for adding time onto an active session

        private Computer _computer;
        private List<Member> _members = new List<Member>();

        // UI Controls
        private ComboBox memberCombo;
        private TextBox nameInput;
        private TextBox txtTimeInput;
        private Button btnAction, btnReserve, btnStop, btnExtend, btnCancel;
        private Label lblTitle, lblStatus, lblRunningBalance;

        private readonly Color _blue = Color.FromArgb(0, 122, 204);

        public SessionModal(Computer computer)
        {
            _computer = computer;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.Size = new Size(460, 520);
            this.Text = $"Manage — {computer.Name}";

            


            InitializeModalUI();
            AddQuickTimeControls();
            this.Load += SessionModal_Load;


        }

        private void InitializeModalUI()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 8, // Increased to 8 to fit the new quick-add row
                BackColor = Color.Transparent
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // [Row 0] Title
            lblTitle = new Label { Text = $"{_computer.Name} Control Panel", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // [Row 1] Status
            string displayStatus = _computer.Status == "in_use" ? "IN USE" : _computer.Status.ToUpper();
            lblStatus = new Label { Text = $"Current Status: {displayStatus}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Fill };
            mainLayout.Controls.Add(lblStatus, 0, 1);

            // [Row 2] Member Dropdown
            memberCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            memberCombo.Items.Add("Loading members list...");
            memberCombo.SelectedIndex = 0;
            memberCombo.Enabled = false;
            memberCombo.SelectedIndexChanged += MemberCombo_SelectedIndexChanged;
            var memberPanel = new Panel { Dock = DockStyle.Fill, Height = 55 };
            memberPanel.Controls.Add(new Label { Text = "Select Member Profile:", ForeColor = Color.White, Dock = DockStyle.Top });
            memberPanel.Controls.Add(memberCombo);
            mainLayout.Controls.Add(memberPanel, 0, 2);

            // [Row 3] Balance
            lblRunningBalance = new Label { Text = "No profile balance loaded.", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(34, 197, 94), Dock = DockStyle.Fill, Visible = false };
            mainLayout.Controls.Add(lblRunningBalance, 0, 3);

            // [Row 4] Name Input
            nameInput = new TextBox { Dock = DockStyle.Top, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var namePanel = new Panel { Dock = DockStyle.Fill, Height = 55 };
            namePanel.Controls.Add(new Label { Text = "Customer Name:", ForeColor = Color.White, Dock = DockStyle.Top });
            namePanel.Controls.Add(nameInput);
            mainLayout.Controls.Add(namePanel, 0, 4);

            // [Row 5] Time Input
            txtTimeInput = new TextBox { Dock = DockStyle.Top, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Text = "0" };
            var timePanel = new Panel { Dock = DockStyle.Fill, Height = 55 };
            timePanel.Controls.Add(new Label { Text = "Duration (Hours):", ForeColor = Color.White, Dock = DockStyle.Top });
            timePanel.Controls.Add(txtTimeInput);
            mainLayout.Controls.Add(timePanel, 0, 5);

            // [Row 6] Quick Add Buttons (New Integration)
            mainLayout.Controls.Add(CreateQuickButtonPanel(), 0, 6);

            // [Row 7] Bottom Action Buttons
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = new Size(70, 32), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAction = new Button { Size = new Size(110, 32), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReserve = new Button { Size = new Size(95, 32), Visible = false, FlatStyle = FlatStyle.Flat };
            btnStop = new Button { Text = "Stop Time", Size = new Size(95, 32), Visible = false, BackColor = Color.FromArgb(220, 38, 38), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnStop.Click += (s, e) => HandleSubmit(Action.StopTime);
            btnExtend = new Button { Text = "Extend", Size = new Size(95, 32), Visible = false, BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            buttonPanel.Controls.AddRange(new Control[] { btnExtend, btnStop, btnReserve, btnAction, btnCancel });
            mainLayout.Controls.Add(buttonPanel, 0, 7);

            SetupActionButtonState();
            this.Controls.Add(mainLayout);
        }

        private FlowLayoutPanel CreateQuickButtonPanel()
        {
            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Height = 40 };

            Button CreateBtn(string text, int minutes)
            {
                var btn = new Button { Text = text, Width = 80, Height = 30, BackColor = _blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btn.Click += (s, e) => {
                    if (double.TryParse(txtTimeInput.Text, out double cur))
                        txtTimeInput.Text = Math.Round(cur + (minutes / 60.0), 2).ToString();
                };
                return btn;
            }

            btnPanel.Controls.Add(CreateBtn("+30m", 30));
            btnPanel.Controls.Add(CreateBtn("+1h", 60));
            btnPanel.Controls.Add(CreateBtn("+2h", 120));
            return btnPanel;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SessionModal
            // 
            this.ClientSize = new System.Drawing.Size(315, 313);
            this.Name = "SessionModal";
            this.ResumeLayout(false);

        }

        private void SetupActionButtonState()
        {
            if (_computer.Status == "available")
            {
                btnAction.Text = "Start Session";
                btnAction.BackColor = Color.FromArgb(34, 197, 94);
                btnAction.Click += (s, e) => HandleSubmit(Action.Start);

                btnReserve.Text = "Reserve PC";
                btnReserve.BackColor = Color.FromArgb(234, 179, 8);
                btnReserve.Visible = true;
                btnReserve.Click += (s, e) => HandleSubmit(Action.Reserve);
            }
            else if (_computer.Status == "in_use")
            {
                btnAction.Text = "Close Panel";
                btnAction.BackColor = Color.FromArgb(71, 85, 105);
                btnAction.Click += (s, e) => this.Close();

                btnStop.Visible = true;
                btnExtend.Visible = true;
                txtTimeInput.Enabled = false;
            }
            else if (_computer.Status == "reserved")
            {
                btnAction.Text = "Claim & Start";
                btnAction.BackColor = Color.FromArgb(34, 197, 94);
                btnAction.Click += (s, e) => HandleSubmit(Action.Start);

                btnReserve.Text = "Release";
                btnReserve.BackColor = Color.FromArgb(148, 163, 184);
                btnReserve.Visible = true;
                btnReserve.Click += (s, e) => HandleSubmit(Action.Free);
            }
        }

        private async void SessionModal_Load(object sender, EventArgs e)
        {
            if (_computer.Status == "reserved" || _computer.Status == "in_use")
            {
                // ... (existing logic for when a PC is already occupied)
                return;
            }

            try
            {
                // Fetch all members AND all computers (to check who is active)
                var allMembers = await SupabaseService.GetMembersAsync();
                var allComputers = await SupabaseService.GetComputersAsync(); // Ensure this method exists

                //  Identify names of members currently active
                var activeCustomerNames = allComputers
                    .Where(c => c.Status == "in_use" && !string.IsNullOrEmpty(c.CurrentCustomer))
                    .Select(c => c.CurrentCustomer)
                    .ToList();

                memberCombo.Items.Clear();
                memberCombo.Items.Add("— Walk-in / Guest Account —");

                // Populate combo only with members NOT in the active list
                foreach (var m in allMembers)
                {
                    // Assuming m.FullName matches the string stored in computer.CurrentCustomer
                    if (!activeCustomerNames.Contains(m.FullName))
                    {
                        _members.Add(m); // Keep track of only available members
                        memberCombo.Items.Add($"👤 {m.FullName} (@{m.Username})");
                    }
                }

                memberCombo.SelectedIndex = 0;
                memberCombo.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading members: " + ex.Message);
            }
        }

        // Update the selection changed handler to use your corrected property
        private void MemberCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (memberCombo.SelectedIndex > 0 && _members.Count >= memberCombo.SelectedIndex)
            {
                var m = _members[memberCombo.SelectedIndex - 1];
                nameInput.Text = m.FullName;
                SelectedMemberId = m.Id.ToString();

                // CORRECTED: Use TimeBalanceSeconds (int) instead of TotalSecondsUsed
                if (m.TimeBalanceSeconds > 0)
                {
                    double unspentHours = m.TimeBalanceSeconds / 3600.0;
                    lblRunningBalance.Text = $"✨ Has saved time balance left: {Math.Round(unspentHours, 2)} hrs remaining.";
                    txtTimeInput.Text = Math.Round(unspentHours, 2).ToString();
                }
                else
                {
                    lblRunningBalance.Text = "Profile loaded. No saved time tokens on record.";
                    txtTimeInput.Text = "0";
                }
                lblRunningBalance.Visible = true;
            }
        }

        private void HandleSubmit(Action action)
        {
            if ((action == Action.Start || action == Action.Reserve) && string.IsNullOrWhiteSpace(CustomerName))
            {
                MessageBox.Show("Please define a valid customer descriptor reference.", "Validation Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (double.TryParse(txtTimeInput.Text.Trim(), out double parsedVal) && parsedVal > 0)
            {
                SelectedHours = parsedVal;
            }
            else
            {
                SelectedHours = -1;
            }

            SelectedAction = action;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Standard custom prompt dialog layout to replace VisualBasic interactions cleanly
        private string PromptExtensionDialog(string title, string instructionText)
        {
            Form prompt = new Form()
            {
                Width = 360,
                Height = 170,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(15, 23, 42),
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 15, Text = instructionText, Width = 310, ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
            TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 300, Text = "1", Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Button confirmation = new Button() { Text = "Confirm", Left = 130, Width = 90, Top = 85, Height = 30, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(79, 70, 229) };
            Button cancelation = new Button() { Text = "Cancel", Left = 230, Width = 90, Top = 85, Height = 30, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(71, 85, 105) };

            confirmation.FlatAppearance.BorderSize = 0;
            cancelation.FlatAppearance.BorderSize = 0;

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancelation);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancelation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
        // Inside your SessionModal constructor or Layout method
        private void AddQuickTimeControls()
        {
            var btnPanel = new FlowLayoutPanel { Top = 200, Left = 20, Width = 300, Height = 50 };

            // Helper to create buttons
            Button CreateQuickBtn(string text, int minutes)
            {
                var btn = new Button { Text = text, Width = 80, Height = 30, BackColor = _blue, ForeColor = Color.White };
                // Change inside your CreateQuickBtn method:
                btn.Click += (s, e) => {
                    if (double.TryParse(txtTimeInput.Text, out double currentHours))
                    {
                        double addedHours = minutes / 60.0;
                        txtTimeInput.Text = Math.Round(currentHours + addedHours, 2).ToString();
                    }
                };
                return btn;
            }

            btnPanel.Controls.Add(CreateQuickBtn("+30m", 30));
            btnPanel.Controls.Add(CreateQuickBtn("+1h", 60));
            btnPanel.Controls.Add(CreateQuickBtn("+2h", 120));

            this.Controls.Add(btnPanel);
        }
    }
}