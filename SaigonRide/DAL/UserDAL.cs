using System.Data.SqlClient;
using SaigonRide.Database;
using SaigonRide.Models;

namespace SaigonRide.DAL
{
    /// <summary>Data Access Layer for Users table.</summary>
    public class UserDAL
    {
        public User? GetByCredentials(string username, string passwordHash)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT UserID,Username,PasswordHash,UserType,PassportID " +
                "FROM Users WHERE Username=@u AND PasswordHash=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return MapUser(r);
        }

        public bool UsernameExists(string username)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE Username=@u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public bool Insert(User u)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Users(Username,PasswordHash,UserType,PassportID) " +
                "VALUES(@u,@p,@t,@passport)", conn);
            cmd.Parameters.AddWithValue("@u",        u.Username);
            cmd.Parameters.AddWithValue("@p",        u.PasswordHash);
            cmd.Parameters.AddWithValue("@t",        u.UserType);
            cmd.Parameters.AddWithValue("@passport", (object?)u.PassportID ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static User MapUser(SqlDataReader r) => new()
        {
            UserID       = (int)r["UserID"],
            Username     = (string)r["Username"],
            PasswordHash = (string)r["PasswordHash"],
            UserType     = (string)r["UserType"],
            PassportID   = r["PassportID"] as string
        };
    }
}
