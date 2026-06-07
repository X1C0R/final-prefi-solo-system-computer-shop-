using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ComputerDashboard;

namespace computerShop
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        // Change this line:
        public Employee AuthenticatedEmployee { get; private set; }

        public LoginForm()
        {
            this.Text = "Employee Login";
            this.Size = new Size(350, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42); // Dark Navy

            InitializeUI();
        }

        private void InitializeUI()
        {

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            var lblTitle = new Label { Text = "Welcome!!!!", Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(60, 30) };

            // Pass 'pnl' into the method so you can add controls to it directly
            txtUsername = CreateStyledTextBox(pnl, 140, "Username");
            txtPassword = CreateStyledTextBox(pnl, 200, "Password");
            txtPassword.UseSystemPasswordChar = true;

            btnLogin = new Button { Text = "Login", Size = new Size(270, 45), Location = new Point(30, 250), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            UiHelper.MakeRounded(btnLogin, 10);
            pnl.Controls.AddRange(new Control[] { lblTitle, txtUsername, txtPassword, btnLogin });
            this.Controls.Add(pnl);
        }

        // Accept the panel as a parameter
        private TextBox CreateStyledTextBox(Panel parentPanel, int top, string labelText)
        {
            // Create and add the label directly to the panel provided
            var lbl = new Label
            {
                Text = labelText,
                ForeColor = Color.LightGray,
                Location = new Point(30, top - 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            parentPanel.Controls.Add(lbl);

            // Return the textbox
            var txt = new TextBox
            {
                Location = new Point(30, top),
                Size = new Size(270, 30),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };

            // Remember to add this txt to the parentPanel too!
            parentPanel.Controls.Add(txt);

            return txt;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "Authenticating...";

            try
            {
                // Validate against the new Employee service method
                var employee = await SupabaseService.LoginEmployeeAsync(txtUsername.Text, txtPassword.Text);

                if (employee != null)
                {
                    this.AuthenticatedEmployee = employee;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid employee credentials.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Login";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
                btnLogin.Enabled = true;
            }
        }
    }
}