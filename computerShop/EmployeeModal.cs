using ComputerDashboard;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace computerShop
{
    public partial class EmployeeModal : Form
    {
        // Class-level fields to access these controls in the Save button event
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private ComboBox cmbRole;

        public EmployeeModal()
        {
            InitializeComponent();

            this.BackColor = Color.FromArgb(15, 23, 42);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Size = new Size(320, 520); // Increased height to accommodate new fields

            SetupUI();
        }

        private void SetupUI()
        {
            int startX = 30;
            int inputWidth = 240;
            int spacing = 65;
            int currentY = 20;

            // Helper to create labels and textboxes
            Func<string, TextBox> createField = (labelText) =>
            {
                var lbl = new Label { Text = labelText, ForeColor = Color.White, Location = new Point(startX, currentY), AutoSize = true, Font = new Font("Segoe UI", 9) };
                this.Controls.Add(lbl);

                var txt = new TextBox
                {
                    Location = new Point(startX, currentY + 20),
                    Size = new Size(inputWidth, 30),
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 10)
                };
                this.Controls.Add(txt);
                currentY += spacing;
                return txt;
            };

            // Initialize Fields
            txtFirstName = createField("First Name");
            txtLastName = createField("Last Name");
            txtUsername = createField("Username");
            txtEmail = createField("Email");

            // Password Field
            var lblPass = new Label { Text = "Password", ForeColor = Color.White, Location = new Point(startX, currentY), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtPassword = new TextBox
            {
                Location = new Point(startX, currentY + 20),
                Size = new Size(inputWidth, 30),
                UseSystemPasswordChar = true,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.AddRange(new Control[] { lblPass, txtPassword });
            currentY += spacing;

            // Role Selection
            var lblRole = new Label { Text = "Role", ForeColor = Color.White, Location = new Point(startX, currentY), AutoSize = true, Font = new Font("Segoe UI", 9) };
            cmbRole = new ComboBox
            {
                Location = new Point(startX, currentY + 20),
                Size = new Size(inputWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White
            };
            cmbRole.Items.AddRange(new string[] { "staff"});
            cmbRole.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblRole, cmbRole });
            currentY += 60;

            // Save Button
            var btnSave = new Button
            {
                Text = "Save Employee",
                Location = new Point(startX, currentY),
                Size = new Size(inputWidth, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnSave.Click += async (s, e) => {
                await SupabaseService.AddEmployeeAsync(
                    txtFirstName.Text, txtLastName.Text, txtUsername.Text,
                    txtEmail.Text, txtPassword.Text, cmbRole.SelectedItem.ToString()
                );
                MessageBox.Show("Employee Added!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnSave);
        }
    }
}