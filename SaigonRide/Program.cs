using SaigonRide.Forms;

namespace SaigonRide
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK || login.LoggedInUser == null)
                return;

            Application.Run(new MainDashboard(login.LoggedInUser));
        }
    }
}
