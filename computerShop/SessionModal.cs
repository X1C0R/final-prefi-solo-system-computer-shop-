using ComputerDashboard;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace computerShop
{
    // ✅ Must inherit Form to get ShowDialog()
    public class SessionModal : Form
    {
        private Computer _computer;

        public enum Action { None, Start, End, Reserve, Free }
        public Action SelectedAction { get; private set; } = Action.None;
        public string CustomerName { get; private set; } = "";

        public SessionModal(Computer computer)
        {
            _computer = computer;
            InitUI();
        }

        private void InitUI()
        {
            Text = _computer.Name + " — " + _computer.Status.ToUpper();
            Size = new Size(420, 400);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(15, 23, 42);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // ── Header strip ──────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = StatusColor(_computer.Status),
            };

            var titleLabel = new Label
            {
                Text = _computer.Name,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Width = 250,
                Height = 40,
                Top = 12,
                Left = 20,
                BackColor = Color.Transparent,
            };

            var statusLabel = new Label
            {
                Text = _computer.Status.Replace("_", " ").ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 255, 220),
                AutoSize = false,
                Width = 250,
                Height = 20,
                Top = 55,
                Left = 22,
                BackColor = Color.Transparent,
            };

            header.Controls.Add(titleLabel);
            header.Controls.Add(statusLabel);

            // ── Body ──────────────────────────────────────────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent,
            };

            int y = 16;

            if (_computer.Status == "in_use" || _computer.Status == "reserved")
            {
                AddInfo(body, "Customer:", _computer.CurrentCustomer ?? "—", ref y);

                if (_computer.SessionStart.HasValue && _computer.Status == "in_use")
                {
                    var elapsed = DateTime.UtcNow - _computer.SessionStart.Value;
                    AddInfo(body, "Time Used:",
                        string.Format("{0}h {1}m", (int)elapsed.TotalHours, elapsed.Minutes),
                        ref y);

                    decimal charge = Math.Round((decimal)elapsed.TotalHours * _computer.HourlyRate, 2);
                    AddInfo(body, "Current Charge:",
                        string.Format("₱{0:F2}  (₱{1}/hr)", charge, _computer.HourlyRate),
                        ref y);
                }
            }

            AddInfo(body, "Rate:", string.Format("₱{0}/hour", _computer.HourlyRate), ref y);
            y += 12;

            // ── Customer name input (available only) ──────────────────────────
            TextBox nameInput = null;

            if (_computer.Status == "available")
            {
                var nameLabel = new Label
                {
                    Text = "Customer Name:",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Left = 20,
                    Top = y,
                    Width = 200,
                    Height = 20,
                    BackColor = Color.Transparent,
                };

                nameInput = new TextBox
                {
                    Left = 20,
                    Top = y + 24,
                    Width = 360,
                    Height = 36,
                    Font = new Font("Segoe UI", 11),
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                };

                body.Controls.Add(nameLabel);
                body.Controls.Add(nameInput);
                y += 76;
            }

            // ── Buttons ───────────────────────────────────────────────────────
            var btnPanel = new FlowLayoutPanel
            {
                Left = 40,
                Top = y + 12,
                Width = 370,
                Height = 54,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
            };

            if (_computer.Status == "available")
            {
                var startBtn = MakeBtn("▶  Start Session", Color.FromArgb(0, 122, 204));
                startBtn.Click += (s, e) =>
                {
                    if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.Text))
                    {
                        MessageBox.Show("Enter customer name first.", "Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    CustomerName = nameInput.Text.Trim();
                    SelectedAction = Action.Start;
                    DialogResult = DialogResult.OK;
                };

                var reserveBtn = MakeBtn("📌 Reserve", Color.FromArgb(133, 83, 0));
                reserveBtn.Click += (s, e) =>
                {
                    if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.Text))
                    {
                        MessageBox.Show("Enter customer name first.", "Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    CustomerName = nameInput.Text.Trim();
                    SelectedAction = Action.Reserve;
                    DialogResult = DialogResult.OK;
                };

                btnPanel.Controls.Add(startBtn);
                btnPanel.Controls.Add(reserveBtn);
            }

            if (_computer.Status == "in_use")
            {
                var endBtn = MakeBtn("⏹  End Session", Color.FromArgb(186, 26, 26));
                endBtn.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show(
                        string.Format("End session for {0}?", _computer.CurrentCustomer),
                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                    {
                        SelectedAction = Action.End;
                        DialogResult = DialogResult.OK;
                    }
                };
                btnPanel.Controls.Add(endBtn);
            }

            if (_computer.Status == "reserved")
            {
                var startBtn = MakeBtn("▶  Start Session", Color.FromArgb(0, 122, 204));
                startBtn.Click += (s, e) =>
                {
                    CustomerName = _computer.CurrentCustomer ?? "";
                    SelectedAction = Action.Start;
                    DialogResult = DialogResult.OK;
                };

                var cancelBtn = MakeBtn("✕  Cancel Reserve", Color.FromArgb(100, 116, 139));
                cancelBtn.Click += (s, e) =>
                {
                    SelectedAction = Action.Free;
                    DialogResult = DialogResult.OK;
                };

                btnPanel.Controls.Add(startBtn);
                btnPanel.Controls.Add(cancelBtn);
            }

            //var closeBtn = MakeBtn("Close", Color.FromArgb(51, 65, 85), Color.FromArgb(148, 163, 184));
            //closeBtn.Click += (s, e) => DialogResult = DialogResult.Cancel;
            //btnPanel.Controls.Add(closeBtn);
           

            body.Controls.Add(btnPanel);
            Controls.Add(body);
            Controls.Add(header);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void AddInfo(Panel parent, string label, string value, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Left = 0,
                Top = y,
                Width = 140,
                Height = 22,
                BackColor = Color.Transparent,
            });
            parent.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Left = 145,
                Top = y,
                Width = 220,
                Height = 22,
                BackColor = Color.Transparent,
            });
            y += 30;
        }

        private Button MakeBtn(string text, Color bg, Color fg = default(Color))
        {
            if (fg == default(Color)) fg = Color.White;

            var btn = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Height = 40,
                Width = text.Length * 8 + 24,
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Color StatusColor(string status)
        {
            switch (status)
            {
                case "in_use": return Color.FromArgb(186, 26, 26);
                case "reserved": return Color.FromArgb(133, 83, 0);
                default: return Color.FromArgb(0, 100, 60);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SessionModal
            // 
            this.ClientSize = new System.Drawing.Size(367, 357);
            this.Name = "SessionModal";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Load += new System.EventHandler(this.SessionModal_Load);
            this.ResumeLayout(false);

        }

        private void SessionModal_Load(object sender, EventArgs e)
        {

        }
    }
}