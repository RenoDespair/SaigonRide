using SaigonRide.BLL;
using SaigonRide.DAL;
using SaigonRide.Models;

namespace SaigonRide.Forms
{
    /// <summary>
    /// UC05 — Manage Vehicle Inventory (CRUD).
    /// Owner: Vu Van Minh Hieu — 524K0005.
    /// </summary>
    public class VehicleManagementForm : Form
    {
        private readonly VehicleBLL _bll     = new();
        private readonly StationDAL _staDal  = new();
        private List<Station>       _stations = new();

        // Controls
        private DataGridView dgv        = null!;
        private ComboBox     cmbCatFilter = null!, cmbStatFilter = null!;
        private Button       btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private Label        lblCount = null!;
        private Panel        pnlForm  = null!;

        // Form panel controls
        private ComboBox cmbCat = null!, cmbStat = null!, cmbStation = null!;
        private Button   btnSave = null!, btnCancel = null!;
        private Label    lblFormTitle = null!;
        private int      _editId = 0;

        public VehicleManagementForm()
        {
            Text = "Vehicle Management — UC05 (Hieu)";
            Size = new Size(900, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 249, 250);
            _stations = _staDal.GetAll();
            Build();
            LoadGrid();
        }

        private void Build()
        {
            // ---- Filter bar ----
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(0, 120, 215) };
            var hdr = new Label {
                Text = "🚲  Vehicle Management", Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White, Location = new Point(10, 10), Size = new Size(350, 30) };
            pnlTop.Controls.Add(hdr);
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.White };
            pnlFilter.Controls.Add(MakeLabel("Category:", 8, 14));
            cmbCatFilter = MakeCmb(80, 10, 130);
            cmbCatFilter.Items.AddRange(new[] { "All", "StandardBike", "EScooter" });
            cmbCatFilter.SelectedIndex = 0;
            pnlFilter.Controls.Add(cmbCatFilter);

            pnlFilter.Controls.Add(MakeLabel("Status:", 220, 14));
            cmbStatFilter = MakeCmb(275, 10, 130);
            cmbStatFilter.Items.AddRange(new[] { "All", "Available", "InTransit", "Maintenance" });
            cmbStatFilter.SelectedIndex = 0;
            pnlFilter.Controls.Add(cmbStatFilter);

            var btnSearch = MakeBtn("🔍 Search", 415, 10, 90, Color.FromArgb(0, 120, 215));
            btnSearch.Click += (_, _) => LoadGrid();
            pnlFilter.Controls.Add(btnSearch);

            lblCount = new Label {
                Location = new Point(515, 14), Size = new Size(200, 22),
                Font = new Font("Segoe UI", 9), ForeColor = Color.Gray };
            pnlFilter.Controls.Add(lblCount);
            Controls.Add(pnlFilter);

            // ---- DataGridView ----
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9), ColumnHeadersHeight = 32
            };
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            Controls.Add(dgv);

            // ---- Action bar ----
            var pnlAct = new Panel { Dock = DockStyle.Bottom, Height = 45, BackColor = Color.White };
            btnAdd     = MakeBtn("➕ Add",    10, 8, 90, Color.FromArgb(40, 167, 69));
            btnEdit    = MakeBtn("✏️ Edit",   110, 8, 90, Color.FromArgb(255, 153, 0));
            btnDelete  = MakeBtn("🗑 Delete", 210, 8, 90, Color.FromArgb(220, 53, 69));
            btnRefresh = MakeBtn("🔄 Refresh", 310, 8, 90, Color.Gray);
            btnAdd.Click    += BtnAdd_Click;
            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (_, _) => LoadGrid();
            pnlAct.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            // ---- Inline form panel ----
            pnlForm = new Panel {
                Size = new Size(380, 230), BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle, Visible = false };
            lblFormTitle = new Label {
                Location = new Point(10, 10), Size = new Size(360, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) };
            pnlForm.Controls.Add(lblFormTitle);

            pnlForm.Controls.Add(MakeLabel("Category:", 10, 45));
            cmbCat = MakeCmb(110, 42, 240);
            cmbCat.Items.AddRange(new[] { "StandardBike", "EScooter" });
            cmbCat.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbCat);

            pnlForm.Controls.Add(MakeLabel("Status:", 10, 82));
            cmbStat = MakeCmb(110, 79, 240);
            cmbStat.Items.AddRange(new[] { "Available", "InTransit", "Maintenance" });
            cmbStat.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbStat);

            pnlForm.Controls.Add(MakeLabel("Station:", 10, 119));
            cmbStation = MakeCmb(110, 116, 240);
            cmbStation.Items.Add("(None / In Transit)");
            foreach (var st in _stations) cmbStation.Items.Add($"{st.StationID}: {st.StationName}");
            cmbStation.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbStation);

            btnSave   = MakeBtn("💾 Save",   110, 160, 100, Color.FromArgb(0, 120, 215));
            btnCancel = MakeBtn("✖ Cancel", 220, 160, 100, Color.Gray);
            btnSave.Click   += BtnSave_Click;
            btnCancel.Click += (_, _) => { pnlForm.Visible = false; };
            pnlForm.Controls.AddRange(new Control[] { btnSave, btnCancel });
            // Correct Dock order: Bottom → Fill → Top
            Controls.Add(pnlForm);
            Controls.Add(pnlAct);
            Controls.Add(dgv);
            Controls.Add(pnlFilter);
            Controls.Add(pnlTop);
        }

        private void LoadGrid()
        {
            string? cat  = cmbCatFilter.SelectedItem?.ToString() == "All" ? null : cmbCatFilter.SelectedItem?.ToString();
            string? stat = cmbStatFilter.SelectedItem?.ToString() == "All" ? null : cmbStatFilter.SelectedItem?.ToString();
            var data = _bll.Search(cat, stat);

            dgv.DataSource = null;
            dgv.Columns.Clear();
            dgv.DataSource = data;

            // Rename columns
            if (dgv.Columns.Contains("VehicleID"))   dgv.Columns["VehicleID"]!.HeaderText   = "ID";
            if (dgv.Columns.Contains("Category"))    dgv.Columns["Category"]!.HeaderText     = "Category";
            if (dgv.Columns.Contains("Status"))      dgv.Columns["Status"]!.HeaderText       = "Status";
            if (dgv.Columns.Contains("StationName")) dgv.Columns["StationName"]!.HeaderText  = "Station";
            if (dgv.Columns.Contains("StationID"))   dgv.Columns["StationID"]!.Visible       = false;
            if (dgv.Columns.Contains("RatePerMinute"))  dgv.Columns["RatePerMinute"]!.HeaderText = "Rate/min (VND)";
            if (dgv.Columns.Contains("DisplayCategory")) dgv.Columns["DisplayCategory"]!.Visible = false;

            lblCount.Text = $"{data.Count} vehicle(s) found";

            // Color rows by status
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var status = row.Cells["Status"]?.Value?.ToString();
                row.DefaultCellStyle.BackColor = status switch
                {
                    "InTransit"   => Color.FromArgb(255, 243, 205),
                    "Maintenance" => Color.FromArgb(248, 215, 218),
                    _             => Color.White
                };
            }
        }

        private void BtnAdd_Click(object? s, EventArgs e)
        {
            _editId = 0;
            lblFormTitle.Text = "➕ Add New Vehicle";
            cmbCat.SelectedIndex = 0; cmbStat.SelectedIndex = 0; cmbStation.SelectedIndex = 0;
            PositionForm(); pnlForm.Visible = true;
        }

        private void BtnEdit_Click(object? s, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is not Vehicle v) { MsgWarn("Select a vehicle first."); return; }
            _editId = v.VehicleID;
            lblFormTitle.Text = $"✏️ Edit Vehicle #{_editId}";
            cmbCat.SelectedItem  = v.Category;
            cmbStat.SelectedItem = v.Status;
            // Restore station selection
            cmbStation.SelectedIndex = 0;
            if (v.StationID.HasValue)
                for (int i = 0; i < cmbStation.Items.Count; i++)
                    if (cmbStation.Items[i].ToString()!.StartsWith(v.StationID.Value + ":"))
                    { cmbStation.SelectedIndex = i; break; }
            PositionForm(); pnlForm.Visible = true;
        }

        private void BtnDelete_Click(object? s, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is not Vehicle v) { MsgWarn("Select a vehicle first."); return; }
            if (MessageBox.Show($"Delete Vehicle #{v.VehicleID}?", "Confirm", MessageBoxButtons.YesNo)
                != DialogResult.Yes) return;
            var (ok, msg) = _bll.Remove(v.VehicleID);
            if (ok) { MsgOk(msg); LoadGrid(); } else MsgErr(msg);
        }

        private void BtnSave_Click(object? s, EventArgs e)
        {
            int? stationId = null;
            var sel = cmbStation.SelectedItem?.ToString() ?? "";
            if (sel != "(None / In Transit)")
                stationId = int.Parse(sel.Split(':')[0]);

            string cat  = cmbCat.SelectedItem!.ToString()!;
            string stat = cmbStat.SelectedItem!.ToString()!;

            // E2: Available vehicle must have a station
            if (stat == "Available" && stationId == null)
            { MsgWarn("Available vehicles must be assigned to a station."); return; }

            (bool ok, string msg) result = _editId == 0
                ? _bll.Add(cat, stat, stationId)
                : _bll.Edit(_editId, cat, stat, stationId);

            if (result.ok) { MsgOk(result.msg); pnlForm.Visible = false; LoadGrid(); }
            else MsgErr(result.msg);
        }

        private void PositionForm()
        {
            pnlForm.Location = new Point(
                (ClientSize.Width  - pnlForm.Width)  / 2,
                (ClientSize.Height - pnlForm.Height) / 2);
        }

        // ---- Helpers ----
        private static Label   MakeLabel(string t, int x, int y) =>
            new() { Text = t, Location = new Point(x, y), Size = new Size(98, 22),
                    Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleRight };
        private static ComboBox MakeCmb(int x, int y, int w) =>
            new() { Location = new Point(x, y), Size = new Size(w, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
        private static Button   MakeBtn(string t, int x, int y, int w, Color c) =>
            new() { Text = t, Location = new Point(x, y), Size = new Size(w, 30),
                    BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9) };
        private static void MsgOk(string m)  => MessageBox.Show(m, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private static void MsgErr(string m) => MessageBox.Show(m, "Error",   MessageBoxButtons.OK, MessageBoxIcon.Error);
        private static void MsgWarn(string m)=> MessageBox.Show(m, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
