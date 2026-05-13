using System.Data.SqlClient;

namespace SaigonRide.Database
{
    /// <summary>
    /// DAL helper — single place for connection string.
    /// Edit Server= if your SQL Server instance name differs.
    /// </summary>
    public static class DatabaseHelper
    {
        // -------------------------------------------------------
        // Change "." to your SQL Server instance name if needed.
        // e.g. @"Server=DESKTOP-XXXXX\SQLEXPRESS;..."
        // -------------------------------------------------------
        private const string ConnectionString =
            @"Server=.;Database=SaigonRideDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
