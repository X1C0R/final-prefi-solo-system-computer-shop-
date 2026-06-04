using ComputerDashboard;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    // Inherit from Panel only
    public class EmployeesPanel : Panel
    {
        private DataGridView _dgvEmployees;
        private TextBox _txtSearch;
        private const string SearchPlaceholder = "Search employees...";

        public EmployeesPanel()
        {
            // Initialize UI directly in the constructor
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(15, 23, 42);
            Padding = new Padding(20);

            // 1. Search Bar (Legacy-compatible placeholder)
            _txtSearch = new TextBox
            {
                Text = SearchPlaceholder,
                ForeColor = Color.Gray,
                Size = new Size(300, 30),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtSearch.Enter += (s, e) => {
                if (_txtSearch.Text == SearchPlaceholder) { _txtSearch.Text = ""; _txtSearch.ForeColor = Color.White; }
            };
            _txtSearch.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text)) { _txtSearch.Text = SearchPlaceholder; _txtSearch.ForeColor = Color.Gray; }
            };

            // 2. Add Button
            var btnAdd = new Button
            {
                Text = "+ Add Employee",
                Size = new Size(150, 30),
                Location = new Point(340, 20),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            UiHelper.MakeRounded(btnAdd, 20);

            btnAdd.Click += (s, e) => {
                using (var modal = new EmployeeModal())
                {
                    modal.ShowDialog();
                }
            };

            // 3. Grid
            _dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 400, // Slightly taller
                BackgroundColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = false,

                // Remove standard borders/lines
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(30, 41, 59),

                // Selection/Interaction
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,

                // Style standard rows
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(15, 23, 42),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(0, 122, 204),
                    SelectionForeColor = Color.White,
                    Padding = new Padding(10, 0, 10, 0),
                    Font = new Font("Segoe UI", 10)
                }
            };

            // Style the Header
            _dgvEmployees.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 0, 10, 0)
            };
            _dgvEmployees.EnableHeadersVisualStyles = false; // Required to see custom colors

            // Manual Columns
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FirstName", HeaderText = "First Name" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastName", HeaderText = "Last Name" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Username" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Role" });

            Controls.AddRange(new Control[] { _txtSearch, btnAdd, _dgvEmployees });
        }

        internal async Task LoadEmployeesAsync()
        {
            try
            {
                // Fetch data from Supabase using the service method
                var employees = await SupabaseService.GetEmployeesAsync();

                // Bind the list directly to the DataGridView
                // This automatically creates columns for your properties
                _dgvEmployees.DataSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employees: " + ex.Message);
            }
        }
    }
}