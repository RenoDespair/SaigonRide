using SaigonRide.BLL;
using SaigonRide.DAL;
using SaigonRide.Models;

namespace SaigonRide.Forms
{
    // ============================================================
    // RENTAL FORM — UC04 (Bao)
    // ============================================================
    public class RentalForm : Form
    {
        private readonly RentalBLL  _bll;
        private readonly VehicleBLL _vBll    = new();
        private readonly StationDAL _staDal  = new();
        private readonly User       _user;

        private ComboBox    cmbStartStation = null!, cmbVehicle = null!;
        private DataGridView dgvHistory     = null!;
        private Button       btnStart = null!, btnEnd = null!;
        private Label        lblActive = null!, lblTimer = null!;
        private System.Windows.Forms.Timer _timer = null!;

        public RentalForm(User user)
        {
            _user = user;
            _bll  = new RentalBLL();
            Text  = $"Rental Management — {_user.Username}";
            Size  = new Size(860, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 249, 250);
            Build();
            RefreshUI();
        }

        private void Build()
        {
            // ---- Header ----
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(40, 167, 69) };
            pnlTop.Controls.Add(new Label {
                Text = "🛴  Rental Management — UC04 (Bao)",
                Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
                Location = new Point(10, 10), Size = new Size(500, 30) });
            Controls.Add(pnlTop);

            // ---- Active rental status bar ----
            var pnlStatus = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White, Padding = new Padding(8) };
            lblActive = new Label {
                Location = new Point(8, 8), Size = new Size(550, 22),
                Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray };
            lblTimer = new Label {
                Location = new Point(8, 30), Size = new Size(350, 20),
                Font = new Font("Segoe UI", 9), ForeColor = Color.DarkGray };
            pnlStatus.Controls.AddRange(new Control[] { lblActive, lblTimer });
            Controls.Add(pnlStatus);

            // ---- Start rental panel ----
            var pnlStart = new GroupBox {
                Text = "Start New Rental", Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 115), Size = new Size(400, 130), BackColor = Color.White };

            pnlStart.Controls.Add(ML("Pick-up Station:", 10, 25));
            var stations = _staDal.GetAll();
            cmbStartStation = new ComboBox {
                Location = new Point(130, 22), Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbStartStation.Items.Add("-- Select Station --");
            foreach (var st in stations) cmbStartStation.Items.Add($"{st.StationID}: {st.StationName} ({st.CurrentCount}/{st.Capacity})");
            cmbStartStation.SelectedIndex = 0;
            cmbStartStation.SelectedIndexChanged += (_, _) => LoadAvailableVehicles();
            pnlStart.Controls.Add(cmbStartStation);

            pnlStart.Controls.Add(ML("Vehicle:", 10, 62));
            cmbVehicle = new ComboBox {
                Location = new Point(130, 59), Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            pnlStart.Controls.Add(cmbVehicle);

            btnStart = new Button {
                Text = "▶  Start Rental", Location = new Point(130, 92), Size = new Size(250, 30),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnStart.Click += BtnStart_Click;
            pnlStart.Controls.Add(btnStart);
            Controls.Add(pnlStart);

            // ---- End rental button ----
            btnEnd = new Button {
                Text = "⏹  End Rental & Checkout", Location = new Point(420, 150), Size = new Size(200, 40),
                BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false };
            btnEnd.Click += BtnEnd_Click;
            Controls.Add(btnEnd);

            // ---- History grid ----
            var pnlHist = new GroupBox {
                Text = "Rental History", Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 255), Size = new Size(830, 270), BackColor = Color.White };
            dgvHistory = new DataGridView {
                Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 8) };
            dgvHistory.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8, FontStyle.Bold);
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 230, 210);
            pnlHist.Controls.Add(dgvHistory);
            Controls.Add(pnlHist);

            // Admin-only: Edit/Delete rental buttons
            if (_user.IsAdmin)
            {
                var btnAdmEdit = new Button {
                    Text = "Edit Rental", Location = new Point(640, 220), Size = new Size(100, 30),
                    BackColor = Color.FromArgb(255, 153, 0), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
                btnAdmEdit.Click += BtnAdminEdit_Click;
                var btnAdmDel = new Button {
                    Text = "Delete Rental", Location = new Point(748, 220), Size = new Size(100, 30),
                    BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
                btnAdmDel.Click += BtnAdminDelete_Click;
                Controls.AddRange(new Control[] { btnAdmEdit, btnAdmDel });
            }

            // Timer for elapsed display
            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += (_, _) => UpdateTimerLabel();
            _timer.Start();
        }

        private void RefreshUI()
        {
            var active = _bll.GetActive(_user.UserID);
            btnStart.Enabled = active == null;
            btnEnd.Enabled   = active != null;
            lblActive.Text   = active != null
                ? $"🔴  Active Rental #{active.RentalID} — Vehicle #{active.VehicleID} from {active.StartStationName}"
                : "⚪  No active rental.";
            LoadHistory();
        }

        private void UpdateTimerLabel()
        {
            var active = _bll.GetActive(_user.UserID);
            if (active == null) { lblTimer.Text = ""; return; }
            var elapsed = DateTime.Now - active.StartTime;
            lblTimer.Text = $"   Elapsed: {(int)elapsed.TotalMinutes} min {elapsed.Seconds} sec";
        }

        private void LoadAvailableVehicles()
        {
            cmbVehicle.Items.Clear();
            if (cmbStartStation.SelectedIndex <= 0) return;
            int sid = int.Parse(cmbStartStation.SelectedItem!.ToString()!.Split(':')[0]);
            var vehicles = _vBll.GetAvailableByStation(sid);
            foreach (var v in vehicles)
                cmbVehicle.Items.Add($"{v.VehicleID}: {v.DisplayCategory} ({v.RatePerMinute} VND/min)");
            if (cmbVehicle.Items.Count > 0) cmbVehicle.SelectedIndex = 0;
            else { cmbVehicle.Items.Add("No vehicles available"); cmbVehicle.SelectedIndex = 0; }
        }

        private void BtnStart_Click(object? s, EventArgs e)
        {
            if (cmbStartStation.SelectedIndex <= 0 || cmbVehicle.SelectedIndex < 0)
            { MsgWarn("Select a station and vehicle."); return; }
            if (cmbVehicle.SelectedItem!.ToString()!.StartsWith("No"))
            { MsgWarn("No vehicles available at this station."); return; }

            int stId  = int.Parse(cmbStartStation.SelectedItem!.ToString()!.Split(':')[0]);
            int vidx  = int.Parse(cmbVehicle.SelectedItem!.ToString()!.Split(':')[0]);

            var (ok, msg, _) = _bll.StartRental(_user.UserID, vidx, stId);
            if (ok) { MsgOk(msg); RefreshUI(); }
            else MsgErr(msg);
        }

        private void BtnEnd_Click(object? s, EventArgs e)
        {
            var active = _bll.GetActive(_user.UserID);
            if (active == null) { MsgWarn("No active rental."); return; }
            using var cf = new CheckoutForm(_user, active, _bll);
            if (cf.ShowDialog(this) == DialogResult.OK) RefreshUI();
        }

        private void BtnAdminEdit_Click(object? s, EventArgs e)
        {
            if (dgvHistory.CurrentRow?.DataBoundItem is not Rental rn) return;
            var stations = _staDal.GetAll();
            var cmbSt = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var st in stations) cmbSt.Items.Add($"{st.StationID}: {st.StationName}");
            cmbSt.SelectedIndex = 0;
            var frm = new Form { Text = "Edit Rental", Size = new Size(340, 180),
                StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Text = "End Station:", Location = new Point(10, 20), Size = new Size(100, 22) };
            cmbSt.Location = new Point(115, 18); cmbSt.Size = new Size(200, 25);
            var btn = new Button { Text = "Save", Location = new Point(115, 80), Size = new Size(200, 30),
                BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.Click += (_, _) => {
                int endSt = int.Parse(cmbSt.SelectedItem!.ToString()!.Split(':')[0]);
                _bll.AdminUpdate(rn.RentalID, endSt, rn.PaymentMethod ?? "Cash");
                frm.Close();
                LoadHistory();
            };
            frm.Controls.AddRange(new Control[] { lbl, cmbSt, btn });
            frm.ShowDialog(this);
        }

        private void BtnAdminDelete_Click(object? s, EventArgs e)
        {
            if (dgvHistory.CurrentRow?.DataBoundItem is not Rental rn) return;
            if (MessageBox.Show($"Delete Rental #{rn.RentalID}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            var (ok, msg) = _bll.AdminDelete(rn.RentalID);
            if (ok) { MsgOk(msg); LoadHistory(); } else MsgErr(msg);
        }

        private void LoadHistory()
        {
            var data = _user.IsAdmin ? _bll.GetAll() : _bll.GetByUser(_user.UserID);
            dgvHistory.DataSource = null;
            dgvHistory.DataSource = data;
            // Hide unneeded columns
            foreach (var h in new[] { "UserID","VehicleID","StartStationID","EndStationID","IsActive","DurationMinutes" })
                if (dgvHistory.Columns.Contains(h)) dgvHistory.Columns[h]!.Visible = false;
            if (dgvHistory.Columns.Contains("FinalFare")) dgvHistory.Columns["FinalFare"]!.HeaderText = "Total (VND)";
            if (dgvHistory.Columns.Contains("DiscountApplied")) dgvHistory.Columns["DiscountApplied"]!.HeaderText = "Discount?";
        }

        private static Label ML(string t, int x, int y) =>
            new() { Text = t, Location = new Point(x, y), Size = new Size(118, 22),
                    Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleRight };
        private static void MsgOk(string m)  => MessageBox.Show(m, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private static void MsgErr(string m) => MessageBox.Show(m, "Error",   MessageBoxButtons.OK, MessageBoxIcon.Error);
        private static void MsgWarn(string m)=> MessageBox.Show(m, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    // ============================================================
    // CHECKOUT FORM — UC04 (Bao)
    // ============================================================
    public class CheckoutForm : Form
    {
        private readonly RentalBLL  _bll;
        private readonly User       _user;
        private readonly Rental     _rental;
        private readonly StationDAL _staDal = new();

        private ComboBox cmbReturnStation = null!;
        private Label    lblDuration = null!, lblBase = null!, lblDiscount = null!, lblFinal = null!;
        private Panel    pnlPayment  = null!;
        private Button   btnCalc = null!;
        private decimal  _baseFare, _finalFare;
        private bool     _discount;

        public CheckoutForm(User user, Rental rental, RentalBLL bll)
        {
            _user = user; _rental = rental; _bll = bll;
            Text  = "Checkout — End Rental & Pay";
            Size  = new Size(480, 480); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);
            Build();
        }

        private void Build()
        {
            var title = new Label {
                Text = "💳  Checkout", Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                Location = new Point(20, 15), Size = new Size(430, 30), TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(title);

            var info = new Label {
                Text = $"Rental #{_rental.RentalID}  |  Vehicle #{_rental.VehicleID}  |  Started: {_rental.StartTime:HH:mm:ss}",
                Location = new Point(20, 50), Size = new Size(430, 20), Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(info);

            // Return station
            Controls.Add(ML("Return Station:", 20, 82));
            var stations = _staDal.GetAll();
            cmbReturnStation = new ComboBox {
                Location = new Point(145, 79), Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            foreach (var st in stations)
                cmbReturnStation.Items.Add($"{st.StationID}: {st.StationName} [{st.OccupancyPercent:F0}% full]");
            cmbReturnStation.SelectedIndex = 0;
            Controls.Add(cmbReturnStation);

            btnCalc = new Button {
                Text = "Calculate Fare", Location = new Point(145, 113), Size = new Size(300, 30),
                BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnCalc.Click += BtnCalc_Click;
            Controls.Add(btnCalc);

            // Fare breakdown box
            var pnlFare = new Panel {
                Location = new Point(20, 155), Size = new Size(430, 120),
                BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            lblDuration = MakeFareLabel("Duration: --", 0,  Color.Black);
            lblBase     = MakeFareLabel("Base Fare: --", 26, Color.Black);
            lblDiscount = MakeFareLabel("",              52, Color.OrangeRed);
            lblFinal    = MakeFareLabel("Total: --",     78, Color.FromArgb(40, 167, 69));
            lblFinal.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            pnlFare.Controls.AddRange(new Control[] { lblDuration, lblBase, lblDiscount, lblFinal });
            Controls.Add(pnlFare);

            // Payment buttons
            var pnlHeader = new Label {
                Text = "Select Payment Method:", Location = new Point(20, 285),
                Size = new Size(430, 22), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Controls.Add(pnlHeader);

            pnlPayment = new Panel { Location = new Point(20, 310), Size = new Size(430, 100), BackColor = Color.Transparent };
            Controls.Add(pnlPayment);
            BuildPaymentButtons();
        }

        private void BtnCalc_Click(object? s, EventArgs e)
        {
            int stationId = int.Parse(cmbReturnStation.SelectedItem!.ToString()!.Split(':')[0]);
            try
            {
                var (b, d, f, mins) = _bll.CalculateFare(_rental.RentalID, stationId);
                _baseFare = b; _discount = d; _finalFare = f;
                lblDuration.Text = $"Duration: {mins} minute(s)";
                lblBase.Text     = $"Base Fare: {b:N0} VND";
                lblDiscount.Text = d ? "✅  15% Discount Applied (Low-inventory station)" : "";
                lblFinal.Text    = $"Total Payable: {f:N0} VND";
                btnCalc.Enabled  = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BuildPaymentButtons()
        {
            pnlPayment.Controls.Clear();
            int x = 0, row = 0;
            foreach (var pm in _user.PaymentMethods)
            {
                var btn = new Button {
                    Text = pm, Location = new Point(x, row * 45),
                    Size = new Size(135, 38), FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                string captured = pm;
                btn.Click += (_, _) => Pay(captured);
                pnlPayment.Controls.Add(btn);
                x += 145;
                if (x > 290) { x = 0; row++; }
            }
        }

        private void Pay(string method)
        {
            if (_finalFare == 0) { MessageBox.Show("Please calculate the fare first.", "Warning"); return; }
            int stId = int.Parse(cmbReturnStation.SelectedItem!.ToString()!.Split(':')[0]);
            var (ok, msg) = _bll.CompleteRental(_rental.RentalID, stId, _baseFare, _discount, _finalFare, method);
            if (ok)
            {
                MessageBox.Show($"✅  Payment via {method} confirmed!\n\nTotal paid: {_finalFare:N0} VND\nThank you for using SaigonRide!",
                    "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static Label MakeFareLabel(string t, int top, Color c) => new()
        {
            Text = t, Location = new Point(10, 10 + top), Size = new Size(410, 24),
            Font = new Font("Segoe UI", 10), ForeColor = c
        };
        private static Label ML(string t, int x, int y) =>
            new() { Text = t, Location = new Point(x, y), Size = new Size(120, 22), Font = new Font("Segoe UI", 9) };
    }
}
