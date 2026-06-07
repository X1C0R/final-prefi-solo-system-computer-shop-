using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace computerShop
{
    public class DashboardPanel : Panel
    {
        private Label _valTotal, _valAvailable, _valTaken, _valReserved;
        private FlowLayoutPanel _computerGrid;
        private System.Windows.Forms.Timer _timer;
        private List<Computer> _currentComputers = new List<Computer>();

        private readonly Color _green = Color.FromArgb(34, 197, 94);
        private readonly Color _red = Color.FromArgb(239, 68, 68);
        private readonly Color _blue = Color.FromArgb(0, 122, 204);
        private readonly Color _yellow = Color.FromArgb(234, 179, 8);

        public DashboardPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(15, 23, 42);
            Padding = new Padding(24);
            AutoScroll = true;

            BuildLayout();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 5000; // Refresh every 5 seconds
            _timer.Tick += async (s, e) =>
            {
                try
                {
                    var freshComputers = await SupabaseService.GetComputersAsync();
                    LoadComputers(freshComputers); // Update the grid
                    UpdateTimerLabels();          // Update the session timers
                }
                catch { /* Silently handle errors so the timer keeps running */ }
            };
            _timer.Start();
        }

        public void StopLiveUpdates() => _timer?.Stop();
        public void StartLiveUpdates() => _timer?.Start();

        private void BuildLayout()
        {
            var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Transparent };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var pageTitle = new Label { Text = "Dashboard", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            stack.Controls.Add(pageTitle, 0, 0);

            var cardsRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 8, 0, 8) };
            var (cardTotal, valTotal) = MakeStatCard("Total PCs", "—", _blue);
            var (cardAvailable, valAvailable) = MakeStatCard("Available", "—", _green);
            var (cardTaken, valTaken) = MakeStatCard("In Use", "—", _red);
            var (cardReserved, valReserved) = MakeStatCard("Reserved", "—", _yellow);

            _valTotal = valTotal; _valAvailable = valAvailable; _valTaken = valTaken; _valReserved = valReserved;
            cardsRow.Controls.AddRange(new Control[] { cardTotal, cardAvailable, cardTaken, cardReserved });
            stack.Controls.Add(cardsRow, 0, 1);

            var gridWrapper = new Panel { Dock = DockStyle.Fill };
            var gridLabel = new Label { Text = "Computers Panel", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Top, Height = 32 };
            _computerGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true };
            gridWrapper.Controls.Add(_computerGrid); gridWrapper.Controls.Add(gridLabel);
            stack.Controls.Add(gridWrapper, 0, 2);

            Controls.Add(stack);
        }

        private (Panel card, Label valueLabel) MakeStatCard(string title, string value, Color accentColor)
        {
            var card = new Panel { Width = 180, Height = 96, Margin = new Padding(0, 0, 16, 0) };
            card.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14))
                using (var bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                using (var borderPen = new Pen(accentColor, 1.5f)) { g.FillPath(bgBrush, path); g.DrawPath(borderPen, path); }
            };
            card.Region = RoundedRegion(card.Width, card.Height, 14);

            var strip = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = accentColor };
            var titleLbl = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Dock = DockStyle.Top, Height = 28, Padding = new Padding(12, 8, 0, 0) };
            var valueLbl = new Label { Text = value, Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = accentColor, Dock = DockStyle.Fill, Padding = new Padding(12, 0, 0, 8), TextAlign = ContentAlignment.BottomLeft };
            card.Controls.AddRange(new Control[] { valueLbl, titleLbl, strip });
            return (card, valueLbl);
        }

        public void LoadComputers(List<Computer> computers)
        {
            _currentComputers = computers;
            SafeSet(_valTotal, computers.Count.ToString());
            SafeSet(_valAvailable, computers.FindAll(c => c.Status == "available").Count.ToString());
            SafeSet(_valTaken, computers.FindAll(c => c.Status == "in_use").Count.ToString());
            SafeSet(_valReserved, computers.FindAll(c => c.Status == "reserved").Count.ToString());

            _computerGrid.SuspendLayout();
            _computerGrid.Controls.Clear();
            foreach (var pc in computers) _computerGrid.Controls.Add(MakeComputerCard(pc));
            _computerGrid.ResumeLayout();
        }

        private void UpdateTimerLabels()
        {
            for (int i = 0; i < _computerGrid.Controls.Count; i++)
            {
                var ctrl = _computerGrid.Controls[i];
                if (!(ctrl.Tag is Computer pc) || pc.Status != "in_use" || !pc.SessionStart.HasValue) continue;

                foreach (Control child in ctrl.Controls)
                {
                    if (child.Tag?.ToString() == "timelbl")
                    {
                        TimeSpan elapsed = DateTime.UtcNow - pc.SessionStart.Value;

                        // Parse limits from notes
                        if (!string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
                        {
                            if (double.TryParse(pc.Notes.Replace("LIMIT:", ""), out double allowedHours))
                            {
                                TimeSpan totalAllowed = TimeSpan.FromMinutes(allowedHours * 60);
                                TimeSpan remaining = totalAllowed - elapsed;

                                if (remaining.TotalSeconds <= 0)
                                {
                                    child.Text = "⚠️ TIME EXPIRED";
                                    child.ForeColor = Color.FromArgb(239, 68, 68);
                                }
                                else
                                {
                                    child.ForeColor = Color.FromArgb(252, 211, 77);
                                    child.Text = (int)remaining.TotalHours > 0 ?
                                        $"{(int)remaining.TotalHours}h {remaining.Minutes}m Left" :
                                        $"{remaining.Minutes}m {remaining.Seconds}s Left";
                                }
                                break;
                            }
                        }

                        // Open Session display tracking count upwards
                        child.Text = (int)elapsed.TotalHours > 0 ? $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m" : $"{elapsed.Minutes}m {elapsed.Seconds}s";
                        break;
                    }
                }
            }
        }

        private Panel MakeComputerCard(Computer pc)
        {
            bool isAvailable = pc.Status == "available";
            bool isReserved = pc.Status == "reserved";
            bool isInUse = pc.Status == "in_use";

            Color accent = isAvailable ? _green : isReserved ? _yellow : _red;
            Color lightBg = isAvailable ? Color.FromArgb(20, 50, 30) : isReserved ? Color.FromArgb(40, 35, 15) : Color.FromArgb(45, 20, 20);

            var card = new Panel { Width = 160, Height = 150, Margin = new Padding(8), Cursor = Cursors.Hand, BackColor = lightBg, Tag = pc };
            card.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12))
                using (var bgBrush = new SolidBrush(lightBg))
                using (var borderPen = new Pen(accent, 2f)) { g.FillPath(bgBrush, path); g.DrawPath(borderPen, path); }
            };
            card.Region = RoundedRegion(card.Width, card.Height, 12);

            var strip = new Panel { Height = 6, Dock = DockStyle.Top, BackColor = accent };
            var pcName = new Label { Text = pc.Name, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = accent, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 35 };

            string statusString = isAvailable ? "🟢 Available" : isReserved ? "🟡 Reserved" : "🔴 Renting";
            var statusLbl = new Label { Text = statusString, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = accent, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 22 };

            var customerLbl = new Label { Text = !isAvailable ? (pc.CurrentCustomer ?? "Customer") : "Vacant", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 20 };

            var timeLbl = new Label { Text = "", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(252, 211, 77), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 20, Tag = "timelbl" };

            // If a custom hour limit tag exists on a static load card, label it explicitly
            if (isInUse && !string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
            {
                timeLbl.Text = "Calculating...";
            }
            else if (isReserved && !string.IsNullOrEmpty(pc.Notes) && pc.Notes.StartsWith("LIMIT:"))
            {
                timeLbl.Text = $"Hold: {pc.Notes.Replace("LIMIT:", "")} hrs";
            }

            card.Controls.AddRange(new Control[] { timeLbl, customerLbl, statusLbl, pcName, strip });

            EventHandler cardClickAction = (s, e) => ComputerCardClicked?.Invoke(pc);
            card.Click += cardClickAction;
            foreach (Control child in card.Controls) child.Click += cardClickAction;

            return card;
        }

        public event Action<Computer> ComputerCardClicked;

        private void SafeSet(Label lbl, string text)
        {
            if (lbl == null) return;
            if (lbl.InvokeRequired) { lbl.BeginInvoke((Action)(() => lbl.Text = text)); return; }
            lbl.Text = text;
        }

        private GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure(); return path;
        }
        private Region RoundedRegion(int w, int h, int r) => new Region(RoundRect(new Rectangle(0, 0, w, h), r));
    }
}