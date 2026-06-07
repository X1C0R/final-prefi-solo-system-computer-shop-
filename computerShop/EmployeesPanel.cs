using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    public class EmployeesPanel : Panel
    {
        private DataGridView _dgvEmployees;
        private TextBox _txtSearch;
        private List<Employee> _allEmployees = new List<Employee>();
        private bool _isPlaceholder = true;
        private const string SearchPlaceholder = "Search by name, username, email...";

        public EmployeesPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(15, 23, 42);
            Padding = new Padding(20);

            // ── Search bar ────────────────────────────────────────────────────
            _txtSearch = new TextBox
            {
                Text = SearchPlaceholder,
                ForeColor = Color.Gray,
                Size = new Size(300, 30),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
            };

            _txtSearch.Enter += (s, e) =>
            {
                if (_isPlaceholder)
                {
                    _isPlaceholder = false;
                    _txtSearch.Text = "";
                    _txtSearch.ForeColor = Color.White;
                }
            };
            _txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _isPlaceholder = true;
                    _txtSearch.Text = SearchPlaceholder;
                    _txtSearch.ForeColor = Color.Gray;
                    BindGrid(_allEmployees);
                }
            };
            _txtSearch.TextChanged += (s, e) =>
            {
                if (!_isPlaceholder)
                    FilterEmployees(_txtSearch.Text);
            };

            // ── Add button ────────────────────────────────────────────────────
            var btnAdd = new Button
            {
                Text = "+ Add Employee",
                Size = new Size(150, 30),
                Location = new Point(340, 20),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            UiHelper.MakeRounded(btnAdd, 20);
            btnAdd.Click += (s, e) =>
            {
                using (var modal = new EmployeeModal())
                    modal.ShowDialog();
            };

            // ── DataGridView ──────────────────────────────────────────────────
            _dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 400,
                BackgroundColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(30, 41, 59),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
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

            _dgvEmployees.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 0, 10, 0)
            };
            _dgvEmployees.EnableHeadersVisualStyles = false;

            // ── Columns ───────────────────────────────────────────────────────
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FirstName", HeaderText = "First Name" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastName", HeaderText = "Last Name" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Username" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email" });
            _dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Role" });

            // ✅ Delete button column
            var deleteCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "🗑 Delete",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(186, 26, 26),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(210, 40, 40),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            };
            _dgvEmployees.Columns.Add(deleteCol);

            // ✅ Wire up the delete click
            _dgvEmployees.CellClick += async (s, e) =>
            {
                // Only fire on the delete column, not header
                if (e.RowIndex < 0 || e.ColumnIndex != _dgvEmployees.Columns.Count - 1) return;

                // Get the matching employee from the currently displayed list
                var currentList = (_dgvEmployees.DataSource as System.ComponentModel.BindingList<Employee>);
                if (currentList == null || e.RowIndex >= currentList.Count) return;

                var employee = currentList[e.RowIndex];

                var confirm = MessageBox.Show(
                    $"Delete \"{employee.FirstName} {employee.LastName}\" (@{employee.Username})?\nThis cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                try
                {
                    await SupabaseService.DeleteEmployeeAsync(employee.Id.ToString());

                    // Remove from master list too
                    _allEmployees.RemoveAll(emp => emp.Id == employee.Id);

                    // Re-apply search or refresh full list
                    if (!_isPlaceholder && !string.IsNullOrWhiteSpace(_txtSearch.Text))
                        FilterEmployees(_txtSearch.Text);
                    else
                        BindGrid(_allEmployees);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Controls.AddRange(new Control[] { _txtSearch, btnAdd, _dgvEmployees });
        }

        // ── Filter ────────────────────────────────────────────────────────────
        private void FilterEmployees(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                BindGrid(_allEmployees);
                return;
            }

            var q = query.Trim().ToLower();
            var filtered = _allEmployees.Where(e =>
                (!string.IsNullOrEmpty(e.FirstName) && e.FirstName.ToLower().Contains(q)) ||
                (!string.IsNullOrEmpty(e.LastName) && e.LastName.ToLower().Contains(q)) ||
                (!string.IsNullOrEmpty(e.Username) && e.Username.ToLower().Contains(q)) ||
                (!string.IsNullOrEmpty(e.Email) && e.Email.ToLower().Contains(q))
            ).ToList();

            BindGrid(filtered);
        }

        // ── Bind list to grid ─────────────────────────────────────────────────
        private void BindGrid(List<Employee> employees)
        {
            _dgvEmployees.DataSource = null;
            _dgvEmployees.DataSource = new System.ComponentModel.BindingList<Employee>(employees);
        }

        // ── Load from Supabase ────────────────────────────────────────────────
        internal async Task LoadEmployeesAsync()
        {
            try
            {
                var employees = await SupabaseService.GetEmployeesAsync();
                _allEmployees = employees;

                if (!_isPlaceholder && !string.IsNullOrWhiteSpace(_txtSearch.Text))
                    FilterEmployees(_txtSearch.Text);
                else
                    BindGrid(_allEmployees);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employees: " + ex.Message);
            }
        }
    }
}