using ComputerDashboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    public class ComputersPanel : Panel
    {
        private DataGridView _dgvComputers;
        private TextBox _txtSearch;
        private List<Computer> _allComputers = new List<Computer>();
        private bool _isPlaceholder = true;
        private const string SearchPlaceholder = "Search by name or number...";

        private readonly Color _bg = Color.FromArgb(15, 23, 42);
        private readonly Color _cardBg = Color.FromArgb(30, 41, 59);
        private readonly Color _blue = Color.FromArgb(0, 122, 204);
        private readonly Color _gray = Color.FromArgb(178, 190, 181);
        private readonly Color _red = Color.FromArgb(186, 26, 26);
        private readonly Color _muted = Color.FromArgb(148, 163, 184);

        public ComputersPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = _bg;
            Padding = new Padding(24);
            BuildLayout();
        }

        private void BuildLayout()
        {
            // ── Title ─────────────────────────────────────────────────────────
            var title = new Label
            {
                Text = "Computers",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 0),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            Controls.Add(title);

            // ── Add button ────────────────────────────────────────────────────
            var btnAdd = new Button
            {
                Text = "+ Add Computer",
                Size = new Size(160, 36),
                Location = new Point(0, 40),
                BackColor = _blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            UiHelper.MakeRounded(btnAdd, 18);
            btnAdd.Click += async (s, e) =>
            {
                using (var modal = new AddComputerModal())
                {
                    if (modal.ShowDialog() == DialogResult.OK)
                        await LoadComputersAsync();
                }
            };
            Controls.Add(btnAdd);

            // ── Search bar ────────────────────────────────────────────────────
            _txtSearch = new TextBox
            {
                Text = SearchPlaceholder,
                ForeColor = Color.Gray,
                Size = new Size(300, 32),
                Location = new Point(0, 90),
                BackColor = _cardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
            };
            _txtSearch.Enter += (s, e) =>
            {
                if (_isPlaceholder) { _isPlaceholder = false; _txtSearch.Text = ""; _txtSearch.ForeColor = Color.White; }
            };
            _txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _isPlaceholder = true;
                    _txtSearch.Text = SearchPlaceholder;
                    _txtSearch.ForeColor = Color.Gray;
                    BindGrid(_allComputers);
                }
            };
            _txtSearch.TextChanged += (s, e) => { if (!_isPlaceholder) FilterComputers(_txtSearch.Text); };
            Controls.Add(_txtSearch);

            // ── DataGridView ──────────────────────────────────────────────────
            _dgvComputers = new DataGridView
            {
                Location = new Point(0, 136),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = _bg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = _cardBg,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = _bg, ForeColor = Color.White, SelectionBackColor = _gray, SelectionForeColor = Color.White, Padding = new Padding(8, 0, 8, 0), Font = new Font("Segoe UI", 10) }
            };

            _dgvComputers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = _cardBg, ForeColor = Color.White, SelectionBackColor = _cardBg, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(8, 0, 8, 0) };
            _dgvComputers.EnableHeadersVisualStyles = false;
            _dgvComputers.RowTemplate.Height = 40;

            // 1. Add Data Columns
            _dgvComputers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ComputerNumber", HeaderText = "#", AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 50 });
            _dgvComputers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Name" });
            _dgvComputers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status" });
            _dgvComputers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HourlyRate", HeaderText = "Rate (₱/hr)", AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 100 });
            _dgvComputers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CurrentCustomer", HeaderText = "Current Customer" });

            // 2. Add Edit Button Column
            _dgvComputers.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Text = "✏ Edit", UseColumnTextForButtonValue = true, Width = 80, FlatStyle = FlatStyle.Flat });

            // 3. Add Delete Button Column
            _dgvComputers.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "🗑 Delete",
                UseColumnTextForButtonValue = true,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = _red, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // 4. Events
            _dgvComputers.CellFormatting += (s, e) => { /* Your existing status color logic */ };

            _dgvComputers.CellClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var current = _dgvComputers.DataSource as System.ComponentModel.BindingList<Computer>;
                if (current == null || e.RowIndex >= current.Count) return;
                var pc = current[e.RowIndex];

                // Edit (2nd to last)
                if (e.ColumnIndex == _dgvComputers.Columns.Count - 2)
                {
                    using (var modal = new EditComputerModal(pc)) { if (modal.ShowDialog() == DialogResult.OK) await LoadComputersAsync(); }
                }
                // Delete (Last)
                else if (e.ColumnIndex == _dgvComputers.Columns.Count - 1)
                {
                    if (pc.Status != "available") { MessageBox.Show("Cannot delete."); return; }
                    if (MessageBox.Show("Delete?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        await SupabaseService.DeleteComputerAsync(pc.Id);
                        await LoadComputersAsync();
                    }
                }
            };

            Controls.Add(_dgvComputers);
            this.Resize += (s, e) => _dgvComputers.Size = new Size(Width - Padding.Left - Padding.Right, Height - 136 - Padding.Top - Padding.Bottom);
        }
        

        // ── Filter ────────────────────────────────────────────────────────────
        private void FilterComputers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) { BindGrid(_allComputers); return; }

            var q = query.Trim().ToLower();
            var filtered = _allComputers.Where(c =>
                (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(q)) ||
                c.ComputerNumber.ToString().Contains(q)
            ).ToList();

            BindGrid(filtered);
        }

        // ── Bind ──────────────────────────────────────────────────────────────
        private void BindGrid(List<Computer> computers)
        {
            _dgvComputers.DataSource = null;
            _dgvComputers.DataSource = new System.ComponentModel.BindingList<Computer>(computers);
        }

        // ── Load ──────────────────────────────────────────────────────────────
        public async Task LoadComputersAsync()
        {
            try
            {
                _allComputers = await SupabaseService.GetComputersAsync();

                if (!_isPlaceholder && !string.IsNullOrWhiteSpace(_txtSearch.Text))
                    FilterComputers(_txtSearch.Text);
                else
                    BindGrid(_allComputers);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading computers: " + ex.Message);
            }
        }
    }
}