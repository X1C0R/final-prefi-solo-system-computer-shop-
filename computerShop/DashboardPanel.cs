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
        // ── Stat card labels ──────────────────────────────────────────────────
        private Label _valTotal, _valAvailable, _valTaken;

        // ── Computer grid ─────────────────────────────────────────────────────
        private FlowLayoutPanel _computerGrid;

        // ── Timer for live elapsed time ───────────────────────────────────────
        private System.Windows.Forms.Timer _timer;
        private List<Computer> _currentComputers = new List<Computer>();

        // Colors
        private readonly Color _cardBg = Color.FromArgb(30, 41, 59);
        private readonly Color _pageBg = Color.FromArgb(15, 23, 42);
        private readonly Color _green = Color.FromArgb(34, 197, 94);
        private readonly Color _red = Color.FromArgb(239, 68, 68);
        private readonly Color _blue = Color.FromArgb(0, 122, 204);
        private readonly Color _yellow = Color.FromArgb(234, 179, 8);
        private readonly Color _textMuted = Color.FromArgb(148, 163, 184);
        private readonly Color _textWhite = Color.White;

        public DashboardPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(15, 23, 42);
            Padding = new Padding(24);
            AutoScroll = true;

            BuildLayout();

            // ✅ Live timer — ticks every second to update elapsed time
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += (s, e) => UpdateTimerLabels();
            _timer.Start();
        }

        // ── Build the full layout ─────────────────────────────────────────────
        private void BuildLayout()
        {
            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));   // title
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));  // stat cards
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // grid

            // ── Row 0: Page title ─────────────────────────────────────────────
            var pageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
            };
            stack.Controls.Add(pageTitle, 0, 0);

            // ── Row 1: Stat cards ─────────────────────────────────────────────
            var cardsRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 8),
            };

            var (cardTotal, valTotal) = MakeStatCard("Total PCs", "—", _blue);
            var (cardAvailable, valAvailable) = MakeStatCard("Available", "—", _green);
            var (cardTaken, valTaken) = MakeStatCard("In Use", "—", _red);
            var (cardReserved, valReserved) = MakeStatCard("Reserved", "—", _yellow);

            _valTotal = valTotal;
            _valAvailable = valAvailable;
            _valTaken = valTaken;

            cardsRow.Controls.Add(cardTotal);
            cardsRow.Controls.Add(cardAvailable);
            cardsRow.Controls.Add(cardTaken);
            cardsRow.Controls.Add(cardReserved);
            stack.Controls.Add(cardsRow, 0, 1);

            // ── Row 2: Computer grid ──────────────────────────────────────────
            var gridWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
            };

            var gridLabel = new Label
            {
                Text = "Computers",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.Transparent,
            };

            _computerGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0),
            };

            gridWrapper.Controls.Add(_computerGrid);
            gridWrapper.Controls.Add(gridLabel);
            stack.Controls.Add(gridWrapper, 0, 2);

            Controls.Add(stack);
        }

        // ── Stat card builder ─────────────────────────────────────────────────
        private (Panel card, Label valueLabel) MakeStatCard(
            string title, string value, Color accentColor)
        {
            var card = new Panel
            {
                Width = 180,
                Height = 96,
                Margin = new Padding(0, 0, 16, 0),
                BackColor = Color.FromArgb(30, 41, 59),
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14);
                var bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59));
                var borderPen = new Pen(accentColor, 1.5f);
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
                bgBrush.Dispose();
                borderPen.Dispose();
            };
            card.Region = RoundedRegion(card.Width, card.Height, 14);

            var strip = new Panel
            {
                Width = 4,
                Dock = DockStyle.Left,
                BackColor = accentColor,
            };

            var titleLbl = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(12, 8, 0, 0),
                BackColor = Color.Transparent,
            };

            var valueLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 0, 0, 8),
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.Transparent,
            };

            card.Controls.Add(valueLbl);
            card.Controls.Add(titleLbl);
            card.Controls.Add(strip);

            return (card, valueLbl);
        }

        // ── Public: load computers and render ─────────────────────────────────
        public void LoadComputers(List<Computer> computers)
        {
            _currentComputers = computers; // ✅ save for timer

            int total = computers.Count;
            int available = computers.FindAll(c => c.Status == "available").Count;
            int taken = computers.FindAll(c => c.Status == "in_use").Count;
            int reserved = computers.FindAll(c => c.Status == "reserved").Count;

            SafeSet(_valTotal, total.ToString());
            SafeSet(_valAvailable, available.ToString());
            SafeSet(_valTaken, taken.ToString());

            _computerGrid.SuspendLayout();
            _computerGrid.Controls.Clear();

            foreach (var pc in computers)
                _computerGrid.Controls.Add(MakeComputerCard(pc));

            _computerGrid.ResumeLayout();
        }

        // ── ✅ Timer tick — updates only the time labels every second ──────────
        private void UpdateTimerLabels()
        {
            foreach (Control ctrl in _computerGrid.Controls)
            {
                var pc = ctrl.Tag as Computer;
                if (pc == null) continue;
                if (pc.Status != "in_use" || !pc.SessionStart.HasValue) continue;

                foreach (Control child in ctrl.Controls)
                {
                    if (child.Tag?.ToString() == "timelbl")
                    {
                        var t = DateTime.UtcNow - pc.SessionStart.Value;

                        // ✅ Show hours, minutes, seconds
                        if ((int)t.TotalHours > 0)
                            child.Text = $"{(int)t.TotalHours}h {t.Minutes}m {t.Seconds}s";
                        else if (t.Minutes > 0)
                            child.Text = $"{t.Minutes}m {t.Seconds}s";
                        else
                            child.Text = $"{t.Seconds}s";

                        break;
                    }
                }
            }
        }

        // ── Computer card ─────────────────────────────────────────────────────
        private Panel MakeComputerCard(Computer pc)
        {
            bool isAvailable = pc.Status == "available";
            bool isReserved = pc.Status == "reserved";
            bool isInUse = pc.Status == "in_use";

            Color accent = isAvailable ? _green : isReserved ? _yellow : _red;
            Color lightBg = isAvailable
                                ? Color.FromArgb(20, 50, 30)
                                : isReserved
                                    ? Color.FromArgb(50, 40, 10)
                                    : Color.FromArgb(50, 20, 20);

            var card = new Panel
            {
                Width = 160,
                Height = 150,
                Margin = new Padding(8),
                Cursor = Cursors.Hand,
                BackColor = lightBg,
                Tag = pc,             // ✅ store pc on the card for timer
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
                var bgBrush = new SolidBrush(lightBg);
                var borderPen = new Pen(accent, 2f);
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
                bgBrush.Dispose();
                borderPen.Dispose();
            };
            card.Region = RoundedRegion(card.Width, card.Height, 12);

            // Top color strip
            var strip = new Panel
            {
                Height = 6,
                Dock = DockStyle.Top,
                BackColor = accent,
            };

            // PC name
            var pcName = new Label
            {
                Text = pc.Name,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.Transparent,
            };

            // Status
            string icon = isAvailable ? "🟢" : isReserved ? "🟡" : "🔴";
            var statusLbl = new Label
            {
                Text = icon + "  " + pc.Status.Replace("_", " "),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Color.Transparent,
            };

            // Customer name
            var customerLbl = new Label
            {
                Text = !isAvailable ? (pc.CurrentCustomer ?? "Unknown") : "Free",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 20,
                Padding = new Padding(4, 0, 4, 0),
                BackColor = Color.Transparent,
            };

            // ✅ Elapsed time label — tagged so timer can find it
            string initialTime = "";
            if (isInUse && pc.SessionStart.HasValue)
            {
                var t = DateTime.UtcNow - pc.SessionStart.Value;
                if ((int)t.TotalHours > 0)
                    initialTime = $"{(int)t.TotalHours}h {t.Minutes}m {t.Seconds}s";
                else if (t.Minutes > 0)
                    initialTime = $"{t.Minutes}m {t.Seconds}s";
                else
                    initialTime = $"{t.Seconds}s";
            }

            var timeLbl = new Label
            {
                Text = initialTime,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = isInUse ? Color.FromArgb(252, 211, 77) : Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = Color.Transparent,
                Tag = "timelbl",      // ✅ tag so timer can find it
            };

            // Hourly charge label (in_use only)
            var chargeLbl = new Label
            {
                Text = isInUse && pc.SessionStart.HasValue
                    ? string.Format("₱{0:F2}",
                        Math.Round((decimal)(DateTime.UtcNow - pc.SessionStart.Value).TotalHours
                        * pc.HourlyRate, 2))
                    : "",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 197, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 18,
                BackColor = Color.Transparent,
                Tag = "chargelbl",    // optional for future live charge update
            };

            // Add controls — order matters for Dock.Top (last added = topmost)
            card.Controls.Add(chargeLbl);
            card.Controls.Add(timeLbl);
            card.Controls.Add(customerLbl);
            card.Controls.Add(statusLbl);
            card.Controls.Add(pcName);
            card.Controls.Add(strip);

            // Click — propagate to all child controls
            EventHandler click = (s, e) => ComputerCardClicked?.Invoke(pc);
            card.Click += click;
            strip.Click += click;
            pcName.Click += click;
            statusLbl.Click += click;
            customerLbl.Click += click;
            timeLbl.Click += click;
            chargeLbl.Click += click;

            return card;
        }

        // ── Event Form1 subscribes to ─────────────────────────────────────────
        public event Action<Computer> ComputerCardClicked;

        // ── Dispose: stop timer ───────────────────────────────────────────────
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void SafeSet(Label lbl, string text)
        {
            if (lbl.InvokeRequired)
            {
                lbl.Invoke((Action)(() => lbl.Text = text));
                return;
            }
            lbl.Text = text;
        }

        private GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Region RoundedRegion(int w, int h, int r)
        {
            return new Region(RoundRect(new Rectangle(0, 0, w, h), r));
        }
    }
}