using SaigonRide.BLL;
using SaigonRide.Models;

namespace SaigonRide.Forms
{
    // ============================================================
    // LOGIN FORM
    // ============================================================
    public class LoginForm : Form
    {
        private readonly UserBLL _bll = new();

        private TextBox txtUsername = null!, txtPassword = null!;
        private ComboBox cmbUserType = null!;
        private Button   btnLogin = null!, btnRegister = null!;
        private Label    lblStatus = null!;

        public User? LoggedInUser { get; private set; }

        public LoginForm()
        {
            Text            = "SaigonRide — Login";
            Size            = new Size(420, 360);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Color.FromArgb(245, 245, 245);
            Build();
        }

        private void Build()
        {
            // Title
            var title = new Label {
                Text = "🛵  SaigonRide", Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(20, 20), Size = new Size(370, 40), TextAlign = ContentAlignment.MiddleCenter };
            var sub = new Label {
                Text = "Distributed Vehicle Rental System", Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray, Location = new Point(20, 58), Size = new Size(370, 20),
                TextAlign = ContentAlignment.MiddleCenter };

            var lblU = MakeLabel("Username:", 90);
            txtUsername = MakeTextBox(120);

            var lblP = MakeLabel("Password:", 155);
            txtPassword = MakeTextBox(185);
            txtPassword.PasswordChar = '●';

            var lblT = MakeLabel("User Type:", 220);
            cmbUserType = new ComboBox {
                Location = new Point(130, 220), Size = new Size(230, 25),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbUserType.Items.AddRange(new[] { "Local", "Tourist", "Admin" });
            cmbUserType.SelectedIndex = 0;

            btnLogin = new Button {
                Text = "Login", Location = new Point(130, 265), Size = new Size(110, 35),
                BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button {
                Text = "Register", Location = new Point(250, 265), Size = new Size(110, 35),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
            btnRegister.Click += (_, _) => {
                using var rf = new RegisterForm();
                rf.ShowDialog(this);
            };

            lblStatus = new Label {
                Location = new Point(20, 310), Size = new Size(370, 20),
                TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red };

            Controls.AddRange(new Control[] {
                title, sub, lblU, txtUsername, lblP, txtPassword,
                lblT, cmbUserType, btnLogin, btnRegister, lblStatus });
        }

        private void BtnLogin_Click(object? s, EventArgs e)
        {
            lblStatus.Text = "";
            var (user, err) = _bll.Login(txtUsername.Text.Trim(),
                                          txtPassword.Text);
            if (user == null) { lblStatus.Text = err; return; }
            // Type must match selection (admins can log in via any type selection)
            if (!user.IsAdmin && user.UserType != cmbUserType.SelectedItem?.ToString())
            {
                lblStatus.Text = "User type does not match account.";
                return;
            }
            LoggedInUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static Label MakeLabel(string text, int top) => new()
        {
            Text = text, Location = new Point(20, top + 3), Size = new Size(105, 22),
            Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleRight
        };
        private static TextBox MakeTextBox(int top) => new()
        {
            Location = new Point(130, top), Size = new Size(230, 25), Font = new Font("Segoe UI", 10)
        };
    }

    // ============================================================
    // REGISTER FORM
    // ============================================================
    public class RegisterForm : Form
    {
        private readonly UserBLL _bll = new();
        private TextBox  txtUser = null!, txtPass = null!, txtPassport = null!;
        private ComboBox cmbType = null!;
        private Label    lblPassport = null!, lblStatus = null!;

        public RegisterForm()
        {
            Text = "SaigonRide — Register"; Size = new Size(420, 380);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);
            Build();
        }

        private void Build()
        {
            var title = new Label {
                Text = "Create Account", Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(20, 20), Size = new Size(370, 30),
                TextAlign = ContentAlignment.MiddleCenter };

            var pairs = new (string lbl, int top)[]
            {
                ("Username:", 65), ("Password:", 110), ("User Type:", 155), ("Passport ID:", 200)
            };

            Controls.Add(title);
            int i = 0;
            foreach (var (lbl, top) in pairs)
                Controls.Add(new Label {
                    Text = lbl, Location = new Point(20, top + 3), Size = new Size(105, 22),
                    Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleRight });

            txtUser     = MakeTB(90);
            txtPass     = MakeTB(135); txtPass.PasswordChar = '●';
            cmbType     = new ComboBox {
                Location = new Point(130, 155), Size = new Size(230, 25),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbType.Items.AddRange(new[] { "Local", "Tourist" });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += (_, _) =>
            {
                bool isTourist = cmbType.SelectedItem?.ToString() == "Tourist";
                lblPassport.Visible = isTourist;
                txtPassport.Visible = isTourist;
            };
            lblPassport = new Label {
                Text = "Passport ID:", Location = new Point(20, 203), Size = new Size(105, 22),
                Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleRight,
                Visible = false };
            txtPassport = MakeTB(200); txtPassport.Visible = false;

            var btnReg = new Button {
                Text = "Register", Location = new Point(130, 255), Size = new Size(230, 35),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnReg.Click += BtnReg_Click;

            lblStatus = new Label {
                Location = new Point(20, 300), Size = new Size(370, 40), TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9) };

            Controls.AddRange(new Control[] {
                txtUser, txtPass, cmbType, lblPassport, txtPassport, btnReg, lblStatus });
        }

        private void BtnReg_Click(object? s, EventArgs e)
        {
            bool isTourist = cmbType.SelectedItem?.ToString() == "Tourist";
            var (ok, msg) = _bll.Register(
                txtUser.Text.Trim(), txtPass.Text,
                cmbType.SelectedItem!.ToString()!,
                isTourist ? txtPassport.Text.Trim() : null);
            lblStatus.ForeColor = ok ? Color.Green : Color.Red;
            lblStatus.Text = msg;
            if (ok) { MessageBox.Show(msg, "Success"); Close(); }
        }

        private static TextBox MakeTB(int top) => new()
        {
            Location = new Point(130, top), Size = new Size(230, 25), Font = new Font("Segoe UI", 10)
        };
    }
}
