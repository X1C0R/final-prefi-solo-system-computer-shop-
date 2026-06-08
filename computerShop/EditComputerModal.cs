using System;
using System.Windows.Forms;
using ComputerDashboard;

namespace computerShop
{
    public class EditComputerModal : Form
    {
        private Computer _pc;
        private TextBox txtNumber, txtName, txtRate;

        public EditComputerModal(Computer pc)
        {
            _pc = pc;
            Text = "Edit Computer";
            Size = new System.Drawing.Size(300, 250);
            StartPosition = FormStartPosition.CenterParent;

            txtNumber = new TextBox { Text = pc.ComputerNumber.ToString(), Location = new System.Drawing.Point(20, 20), Width = 240 };
            txtName = new TextBox { Text = pc.Name, Location = new System.Drawing.Point(20, 60), Width = 240 };
            txtRate = new TextBox { Text = pc.HourlyRate.ToString(), Location = new System.Drawing.Point(20, 100), Width = 240 };

            var btnSave = new Button { Text = "Save Changes", Location = new System.Drawing.Point(20, 150), Width = 240 };
            btnSave.Click += async (s, e) => {
                _pc.ComputerNumber = int.Parse(txtNumber.Text);
                _pc.Name = txtName.Text;
                _pc.HourlyRate = decimal.Parse(txtRate.Text);

                await SupabaseService.UpdateComputerAsync(_pc); // Ensure this is in your service
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[] { txtNumber, txtName, txtRate, btnSave });
        }
    }
}