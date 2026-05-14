using SaigonRide.BLL;
using SaigonRide.Models;
using System.Data;

namespace SaigonRide.Forms
{
    // ============================================================
    // REVENUE REPORT FORM — UC06 (Hieu)
    // ============================================================
    public class RevenueReportForm : Form
    {
        private readonly ReportBLL _bll = new();
        private DataGridView dgv       = null!;
        private DateTimePicker dtpFrom = null!, dtpTo = null!;
        private CheckBox chkUseRange   = null!;
        private Label    lblTotal      = null!;

        public RevenueReportForm()
        {
            Text = "Revenue Report by Vehicle Category — UC06 (Hieu)";
            Size = new Size(700, 480);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 249, 250);
            Build();
            LoadReport();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(0, 120, 215) };
            pnlTop.Controls.Add(new Label {
                Text = "📊  Revenue Report by Vehicle Category",
                Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
                Location = new Point(10, 10), Size = new Size(500, 30) });
            Controls.Add(pnlTop);

            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            chkUseRange = new CheckBox {
                Text = "Filter by Date Range:", Location = new Point(8, 14),
                Size = new Size(160, 22), Font = new Font("Segoe UI", 9) };
            chkUseRange.CheckedChanged += (_, _) => {
                dtpFrom.Enabled = chkUseRange.Checked;
                dtpTo.Enabled   = chkUseRange.Checked;
            };
            dtpFrom = new DateTimePicker {
                Location = new Point(175, 11), Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short, Enabled = false };
            var lbl = new Label {
                Text = "to", Location = new Point(330, 14), Size = new Size(25, 22),
                Font = new Font("Segoe UI", 9) };
            dtpTo = new DateTimePicker {
                Location = new Point(355, 11), Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short, Enabled = false, Value = DateTime.Now };
            var btnRun = new Button {
                Text = "▶ Run Report", Location = new Point(515, 10), Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnRun.Click += (_, _) => LoadReport();
            pnlFilter.Controls.AddRange(new Control[] { chkUseRange, dtpFrom, lbl, dtpTo, btnRun });
            Controls.Add(pnlFilter);

            dgv = new DataGridView {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10) };
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
            Controls.Add(dgv);

            var pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White };
            lblTotal = new Label {
                Location = new Point(10, 10), Size = new Size(660, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) };
            pnlBot.Controls.Add(lblTotal);
            Controls.Add(pnlBot);
        }

        private void LoadReport()
        {
            DateTime? from = chkUseRange.Checked ? dtpFrom.Value.Date : null;
            DateTime? to   = chkUseRange.Checked ? dtpTo.Value.Date.AddDays(1) : null;
            var dt = _bll.GetRevenueReport(from, to);

            // Rename columns for display
            if (dt.Columns.Contains("Category"))      dt.Columns["Category"]!.ColumnName      = "Vehicle Category";
            if (dt.Columns.Contains("TotalRentals"))  dt.Columns["TotalRentals"]!.ColumnName  = "Total Rentals";
            if (dt.Columns.Contains("TotalRevenue"))  dt.Columns["TotalRevenue"]!.ColumnName  = "Total Revenue (VND)";

            dgv.DataSource = dt;
            if (dgv.Columns.Contains("Vehicle Category"))
                dgv.Columns["Vehicle Category"]!.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            decimal grand = 0;
            foreach (DataRow row in dt.Rows)
                if (row["Total Revenue (VND)"] != DBNull.Value)
                    grand += Convert.ToDecimal(row["Total Revenue (VND)"]);

            lblTotal.Text = $"Grand Total Revenue: {grand:N0} VND";
        }
    }

    // ============================================================
    // STATION INVENTORY REPORT — UC07 (Bao)
    // ============================================================
    public class StationInventoryForm : Form
    {
        private readonly ReportBLL _bll = new();
        private DataGridView dgv        = null!;

        public StationInventoryForm()
        {
            Text = "Station Inventory Report — UC07 (Bao)";
            Size = new Size(680, 460);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 249, 250);
            Build();
            LoadReport();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(40, 167, 69) };
            pnlTop.Controls.Add(new Label {
                Text = "📍  Station Inventory / Utilization Report",
                Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
                Location = new Point(10, 10), Size = new Size(500, 30) });
            Controls.Add(pnlTop);

            var pnlInfo = new Panel { Dock = DockStyle.Top, Height = 35, BackColor = Color.White };
            pnlInfo.Controls.Add(new Label {
                Text = "⚠️  Stations highlighted in RED are Low Inventory (< 20% full) — qualify for 15% discount",
                Location = new Point(8, 8), Size = new Size(660, 20), Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.DarkRed });
            Controls.Add(pnlInfo);

            dgv = new DataGridView {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10) };
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 230, 210);
            dgv.CellFormatting += Dgv_CellFormatting;
            Controls.Add(dgv);

            var pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White };
            var btnRef = new Button {
                Text = "🔄 Refresh", Location = new Point(10, 8), Size = new Size(100, 28),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
            btnRef.Click += (_, _) => LoadReport();
            pnlBot.Controls.Add(btnRef);
            Controls.Add(pnlBot);
        }

        private void LoadReport()
        {
            var stations = _bll.GetStationInventoryReport();
            // Build display DataTable
            var dt = new DataTable();
            dt.Columns.Add("Station ID",    typeof(int));
            dt.Columns.Add("Station Name",  typeof(string));
            dt.Columns.Add("Capacity",      typeof(int));
            dt.Columns.Add("Current Bikes", typeof(int));
            dt.Columns.Add("% Full",        typeof(string));
            dt.Columns.Add("Status",        typeof(string));

            foreach (var st in stations)
            {
                dt.Rows.Add(
                    st.StationID,
                    st.StationName,
                    st.Capacity,
                    st.CurrentCount,
                    $"{st.OccupancyPercent:F1}%",
                    st.IsLowInventory ? "⚠️ LOW INVENTORY" : "Normal"
                );
            }
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.DataSource = dt;
            if (dgv.Columns.Count > 0) dgv.Columns[0].Width = 80;
            if (dgv.Columns.Count > 1) dgv.Columns[1].Width = 130;
            if (dgv.Columns.Count > 2) dgv.Columns[2].Width = 80;
            if (dgv.Columns.Count > 3) dgv.Columns[3].Width = 90;
            if (dgv.Columns.Count > 4) dgv.Columns[4].Width = 70;
            if (dgv.Columns.Count > 5) dgv.Columns[5].Width = 100;
        }

        private void Dgv_CellFormatting(object? s, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Rows.Count <= e.RowIndex || e.RowIndex < 0) return;
            var statusCell = dgv.Rows[e.RowIndex].Cells["Status"];
            if (statusCell?.Value?.ToString()?.Contains("LOW") == true)
            {
                e.CellStyle!.BackColor = Color.FromArgb(248, 215, 218);
                e.CellStyle.ForeColor  = Color.DarkRed;
            }
        }
    }
}
