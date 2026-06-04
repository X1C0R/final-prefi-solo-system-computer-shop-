using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    public class MembersPanel : Panel
    {
        private FlowLayoutPanel _memberGrid;
        private Label _totalLabel;
        private System.Windows.Forms.Timer _timer;

        // Colors
        private readonly Color _bg = Color.FromArgb(15, 23, 42);
        private readonly Color _cardBg = Color.FromArgb(30, 41, 59);
        private readonly Color _blue = Color.FromArgb(0, 122, 204);
        private readonly Color _green = Color.FromArgb(34, 197, 94);
        private readonly Color _muted = Color.FromArgb(148, 163, 184);

        public MembersPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = _bg;
            Padding = new Padding(24);
            AutoScroll = true;
            BuildLayout();

            _timer = new System.Windows.Forms.Timer { Interval = 5000 };
            _timer.Tick += async (s, e) => await LoadMembersAsync();
            _timer.Start();
        }

        private void BuildLayout()
        {
            var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var title = new Label { Text = "Members", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            stack.Controls.Add(title, 0, 0);

            var addBtn = new Button { Text = "+ Add New Member", Width = 180, Height = 40, BackColor = _blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 8, 0, 8) };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += (s, e) => OpenAddMemberDialog();
            stack.Controls.Add(addBtn, 0, 1);

            _totalLabel = new Label { Text = "Total Members: —", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = _muted, AutoSize = true };
            stack.Controls.Add(_totalLabel, 0, 2);

            _memberGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true, BackColor = Color.Transparent };
            stack.Controls.Add(_memberGrid, 0, 3);

            Controls.Add(stack);
        }

        private void OpenAddMemberDialog()
        {
            // 1. Setup Form with a cleaner look
            var form = new Form
            {
                Text = "Register New Member",
                Size = new Size(360, 420),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = _bg,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // 2. Add a Header Label
            var header = new Label { Text = "Create New Account", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Left = 20, Top = 20, Width = 300 };

            // 3. Helper for clean, uniform textboxes
            TextBox CreateInput(int top) => new TextBox
            {
                Left = 20,
                Top = top,
                Width = 300,
                Height = 30,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };

            var lblName = new Label
            {
                Text = "Full Name",
                ForeColor = _muted,
                Left = 20,
                Top = 60,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.Transparent, 
                AutoSize = true               
            };
            var nameBox = CreateInput(80);

            var lblUser = new Label
            {
                Text = "Username",
                ForeColor = _muted,
                Left = 20,
                Top = 130,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.Transparent, 
                AutoSize = true                
            };
            var userBox = CreateInput(150);

            // 4. Stylized Button
            var saveBtn = new Button
            {
                Text = "Save Member",
                Top = 240,
                Left = 20,
                Width = 300,
                Height = 45,
                BackColor = _blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            saveBtn.FlatAppearance.BorderSize = 0;

            saveBtn.Click += async (s, e) => {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                saveBtn.Enabled = false;
                saveBtn.Text = "Saving...";

                await SupabaseService.AddMemberAsync(nameBox.Text, userBox.Text, "password");
                form.Close();
                await LoadMembersAsync();
            };

            form.Controls.AddRange(new Control[] { header, lblName, nameBox, lblUser, userBox, saveBtn });
            form.ShowDialog();
        }

        public async Task LoadMembersAsync()
        {
            try
            {
                var members = await SupabaseService.GetMembersAsync();
                this.Invoke((Action)(() => {
                    _totalLabel.Text = $"Total Members: {members.Count}";
                    RebuildGrid(members);
                }));
            }
            catch { }
        }

        private void RebuildGrid(List<Member> members)
        {
            _memberGrid.SuspendLayout();
            _memberGrid.Controls.Clear();
            foreach (var m in members) _memberGrid.Controls.Add(MakeMemberCard(m));
            _memberGrid.ResumeLayout();
        }

        private Panel MakeMemberCard(Member member)
        {
            var card = new Panel { Width = 200, Height = 140, Margin = new Padding(12), BackColor = _cardBg };

            // Modern Top Border Strip
            var strip = new Panel { Height = 4, Dock = DockStyle.Top, BackColor = _blue };
            card.Controls.Add(strip);

            // Member Info
            var name = new Label
            {
                Text = member.FullName,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Top = 30, // Adjusted top position since the button is gone
                Width = 200,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var time = new Label
            {
                Text = $"⏱ {Math.Round(member.TimeBalanceSeconds / 3600.0, 2)}h",
                Font = new Font("Segoe UI", 9),
                ForeColor = _green,
                Top = 60, // Adjusted top position since the button is gone
                Width = 200,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add only the label controls
            card.Controls.AddRange(new Control[] { name, time });

            return card;
        }

      


    }
}