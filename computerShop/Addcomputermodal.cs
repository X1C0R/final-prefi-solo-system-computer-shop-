using ComputerDashboard;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace computerShop
{
    public class AddComputerModal : Form
    {
        private readonly Color _bg = Color.FromArgb(15, 23, 42);
        private readonly Color _cardBg = Color.FromArgb(30, 41, 59);
        private readonly Color _blue = Color.FromArgb(0, 122, 204);
        private readonly Color _muted = Color.FromArgb(148, 163, 184);

        public AddComputerModal()
        {
            Text = "Add New Computer";
            Size = new Size(400, 380);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = _bg;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildUI();
        }

        private void BuildUI()
        {
            int y = 20;

            var header = new Label
            {
                Text = "Add New Computer",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 20,
                Top = y,
                Width = 340,
                AutoSize = true,
            };
            Controls.Add(header);
            y += 44;

            Controls.Add(MakeLabel("Computer Name  (e.g. PC-01)", y));
            y += 22;
            var nameBox = MakeInput(y); Controls.Add(nameBox); y += 44;

            Controls.Add(MakeLabel("Computer Number  (unique integer)", y));
            y += 22;
            var numberBox = MakeInput(y); Controls.Add(numberBox); y += 44;

            Controls.Add(MakeLabel("Hourly Rate  (₱)", y));
            y += 22;
            var rateBox = MakeInput(y);
            rateBox.Text = "30";
            Controls.Add(rateBox);
            y += 52;

            var saveBtn = new Button
            {
                Text = "Add Computer",
                Left = 20,
                Top = y,
                Width = 340,
                Height = 44,
                BackColor = _blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            UiHelper.MakeRounded(saveBtn, 18);

            saveBtn.Click += async (s, e) =>
            {
                // 1. Basic Validation
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Computer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(numberBox.Text.Trim(), out int pcNumber) || pcNumber <= 0)
                {
                    MessageBox.Show("Computer number must be a positive integer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(rateBox.Text.Trim(), out decimal rate) || rate < 0)
                {
                    MessageBox.Show("Hourly rate must be a valid number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Duplicate Check
                string newName = nameBox.Text.Trim();
                var existingComputers = await SupabaseService.GetComputersAsync();

                bool nameExists = existingComputers.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                bool numberExists = existingComputers.Any(c => c.ComputerNumber == pcNumber);

                if (nameExists || numberExists)
                {
                    string message = nameExists && numberExists ?
                        $"Both name \"{newName}\" and number {pcNumber} are already in use!" :
                        (nameExists ? $"A computer named \"{newName}\" already exists!" : $"Computer number {pcNumber} is already assigned.");

                    MessageBox.Show(message, "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. Save
                saveBtn.Enabled = false;
                saveBtn.Text = "Saving...";

                try
                {
                    await SupabaseService.AddComputerAsync(newName, pcNumber, rate);

                    MessageBox.Show($"✅ \"{newName}\" added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add computer:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    saveBtn.Enabled = true;
                    saveBtn.Text = "Add Computer";
                }
            };

            Controls.Add(saveBtn);
        }

        private Label MakeLabel(string text, int top) => new Label
        {
            Text = text,
            ForeColor = _muted,
            Left = 20,
            Top = top,
            Width = 340,
            Font = new Font("Segoe UI", 8),
            BackColor = Color.Transparent,
            AutoSize = true,
        };

        private TextBox MakeInput(int top) => new TextBox
        {
            Left = 20,
            Top = top,
            Width = 340,
            Height = 32,
            BackColor = _cardBg,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10),
        };
    }
}