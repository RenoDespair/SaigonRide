using SaigonRide.Models;

namespace SaigonRide.Forms
{
    // ============================================================
    // MAIN DASHBOARD
    // ============================================================
    public class MainDashboard : Form
    {
        private readonly User _user;

        public MainDashboard(User user)
        {
            _user = user;
            Text  = $"SaigonRide — Dashboard  [{_user.Username} / {_user.UserType}]";
            Size = new Size(700, 620);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(700, 620);
            BackColor = Color.FromArgb(245, 247, 250);
            Build();
        }

        private void Build()
        {
            // ---- Welcome panel ----
            var pnlWelcome = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlWelcome.Controls.Add(new Label
            {
                Text = $"Welcome back, {_user.Username}! Select a module below.",
                Location = new Point(20, 18),
                Size = new Size(600, 28),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(60, 60, 60)
            });
            Controls.Add(pnlWelcome);

            // ---- Top bar ----
            var pnlTop = new Panel {
                Dock = DockStyle.Top, Height = 60,
                BackColor = Color.FromArgb(0, 120, 215) };
            var lblApp = new Label {
                Text = "🛵  SaigonRide — Distributed Vehicle Rental System",
                Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
                Location = new Point(15, 10), Size = new Size(560, 30) };
            var lblUser = new Label {
                Text = $"👤  {_user.Username}  |  {_user.UserType}",
                Font = new Font("Segoe UI", 9), ForeColor = Color.LightBlue,
                Location = new Point(15, 38), Size = new Size(560, 18) };
            var btnLogout = new Button {
                Text = "Logout", Location = new Point(600, 14), Size = new Size(80, 32),
                BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
            btnLogout.Click += (_, _) => { Close(); };
            pnlTop.Controls.AddRange(new Control[] { lblApp, lblUser, btnLogout });
            Controls.Add(pnlTop);

            // ---- Module grid ----
            var pnlModules = new FlowLayoutPanel {
                Dock = DockStyle.Fill, Padding = new Padding(20, 15, 20, 15),
                AutoScroll = true, BackColor = Color.FromArgb(245, 247, 250) };
            Controls.Add(pnlModules);

            void AddModule(string icon, string title, string desc, Color color, Action onClick, bool enabled = true)
            {
                var card = new Panel {
                    Size = new Size(195, 165), BackColor = Color.White,
                    Margin = new Padding(10),
                    Cursor = enabled ? Cursors.Hand : Cursors.Default };
                card.MouseEnter += (_, _) => { if (enabled) card.BackColor = Color.FromArgb(240, 248, 255); };
                card.MouseLeave += (_, _) => card.BackColor = Color.White;
                card.MouseClick += (_, _) => { if (enabled) onClick(); };

                var stripe = new Panel { Size = new Size(195, 6), BackColor = color, Location = new Point(0, 0) };
                var ico    = new Label { Text = icon, Location = new Point(15, 18), Size = new Size(50, 45),
                    Font = new Font("Segoe UI Emoji", 24), TextAlign = ContentAlignment.MiddleCenter };
                var lTitle = new Label { Text = title, Location = new Point(65, 22), Size = new Size(120, 22),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = enabled ? Color.Black : Color.Gray };
                var lDesc  = new Label { Text = desc,  Location = new Point(15, 78), Size = new Size(165, 50),
                    Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, AutoSize = false };
                if (!enabled) { lTitle.Text += "\n(Admin only)"; }
                var btn = new Button {
                    Text = enabled ? "Open →" : "Restricted", Location = new Point(15, 130),
                    Size = new Size(165, 28), BackColor = enabled ? color : Color.LightGray,
                    ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold) };
                btn.Click += (_, _) => { if (enabled) onClick(); };
                card.Controls.AddRange(new Control[] { stripe, ico, lTitle, lDesc, btn });
                pnlModules.Controls.Add(card);
                pnlModules.AutoScrollPosition = new Point(0, 0);
            }

            // ---- Modules by user type ----
            bool isAdmin = _user.IsAdmin;

            AddModule("🚲", "Manage Vehicles", "Add, edit, delete, search vehicles. (UC05 — Hieu)",
                Color.FromArgb(0, 120, 215), OpenVehicles, isAdmin);

            AddModule("🛴", "My Rentals", "Start and end rentals, view history. (UC04 — Bao)",
                Color.FromArgb(40, 167, 69), OpenRentals);

            AddModule("📊", "Revenue Report", "Revenue by vehicle category. (UC06 — Hieu)",
                Color.FromArgb(255, 153, 0), OpenRevenue, isAdmin);

            AddModule("📍", "Station Report", "Station inventory utilization. (UC07 — Bao)",
                Color.FromArgb(102, 16, 242), OpenStation);

            if (isAdmin)
                AddModule("🗂", "All Rentals", "Admin: view, edit, delete all rentals.",
                    Color.FromArgb(220, 53, 69), OpenRentals);
        }

        private void OpenVehicles() { using var f = new VehicleManagementForm(); f.ShowDialog(this); }
        private void OpenRentals()  { using var f = new RentalForm(_user);        f.ShowDialog(this); }
        private void OpenRevenue()  { using var f = new RevenueReportForm();      f.ShowDialog(this); }
        private void OpenStation()  { using var f = new StationInventoryForm();   f.ShowDialog(this); }
    }
}
