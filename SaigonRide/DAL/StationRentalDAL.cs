using System.Data.SqlClient;
using SaigonRide.Database;
using SaigonRide.Models;
using System.Data;

namespace SaigonRide.DAL
{
    // ============================================================
    // STATION DAL
    // ============================================================
    public class StationDAL
    {
        public List<Station> GetAll()
        {
            var list = new List<Station>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT StationID,StationName,Capacity,CurrentCount FROM Stations ORDER BY StationID", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapStation(r));
            return list;
        }

        public Station? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT StationID,StationName,Capacity,CurrentCount FROM Stations WHERE StationID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapStation(r) : null;
        }

        /// <summary>Recalculates CurrentCount from actual vehicle records.</summary>
        public void RefreshCount(int stationId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Stations SET CurrentCount=(" +
                "SELECT COUNT(*) FROM Vehicles " +
                "WHERE StationID=@sid AND Status='Available') " +
                "WHERE StationID=@sid", conn);
            cmd.Parameters.AddWithValue("@sid", stationId);
            cmd.ExecuteNonQuery();
        }

        private static Station MapStation(SqlDataReader r) => new()
        {
            StationID    = (int)r["StationID"],
            StationName  = (string)r["StationName"],
            Capacity     = (int)r["Capacity"],
            CurrentCount = (int)r["CurrentCount"]
        };
    }

    // ============================================================
    // RENTAL DAL — owned by Nguyen Gia Bao (UC04)
    // ============================================================
    public class RentalDAL
    {
        // ---- READ ------------------------------------------------
        public List<Rental> GetAll()
        {
            var list = new List<Rental>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(GetJoinQuery(""), conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapRental(r));
            return list;
        }

        public List<Rental> GetByUser(int userId)
        {
            var list = new List<Rental>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(GetJoinQuery("WHERE rn.UserID=@uid"), conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapRental(r));
            return list;
        }

        public Rental? GetActiveByUser(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                GetJoinQuery("WHERE rn.UserID=@uid AND rn.EndTime IS NULL"), conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapRental(r) : null;
        }

        public Rental? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(GetJoinQuery("WHERE rn.RentalID=@id"), conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapRental(r) : null;
        }

        // ---- CREATE ----------------------------------------------
        public int Insert(Rental rn)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Rentals(UserID,VehicleID,StartStationID,StartTime) " +
                "VALUES(@uid,@vid,@ssid,@st); SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@uid",  rn.UserID);
            cmd.Parameters.AddWithValue("@vid",  rn.VehicleID);
            cmd.Parameters.AddWithValue("@ssid", rn.StartStationID);
            cmd.Parameters.AddWithValue("@st",   rn.StartTime);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ---- UPDATE (end rental / complete payment) --------------
        public bool Complete(int rentalId, int endStationId, DateTime endTime,
                             decimal baseFare, bool discountApplied, decimal finalFare,
                             string paymentMethod)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Rentals SET EndStationID=@esid,EndTime=@et,BaseFare=@bf," +
                "DiscountApplied=@da,FinalFare=@ff,PaymentMethod=@pm " +
                "WHERE RentalID=@rid", conn);
            cmd.Parameters.AddWithValue("@esid", endStationId);
            cmd.Parameters.AddWithValue("@et",   endTime);
            cmd.Parameters.AddWithValue("@bf",   baseFare);
            cmd.Parameters.AddWithValue("@da",   discountApplied);
            cmd.Parameters.AddWithValue("@ff",   finalFare);
            cmd.Parameters.AddWithValue("@pm",   paymentMethod);
            cmd.Parameters.AddWithValue("@rid",  rentalId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateAdmin(int rentalId, int endStationId, string paymentMethod)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Rentals SET EndStationID=@esid,PaymentMethod=@pm WHERE RentalID=@rid", conn);
            cmd.Parameters.AddWithValue("@esid", endStationId);
            cmd.Parameters.AddWithValue("@pm",   paymentMethod);
            cmd.Parameters.AddWithValue("@rid",  rentalId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ---- DELETE ----------------------------------------------
        public bool Delete(int rentalId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM Rentals WHERE RentalID=@id", conn);
            cmd.Parameters.AddWithValue("@id", rentalId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ---- REPORTS (used by ReportBLL) -------------------------
        public DataTable GetRevenueByCategory(DateTime? from, DateTime? to)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var sql = "SELECT v.Category, COUNT(*) AS TotalRentals, " +
                      "SUM(rn.FinalFare) AS TotalRevenue " +
                      "FROM Rentals rn JOIN Vehicles v ON rn.VehicleID=v.VehicleID " +
                      "WHERE rn.FinalFare IS NOT NULL";
            if (from.HasValue) sql += " AND rn.EndTime >= @from";
            if (to.HasValue)   sql += " AND rn.EndTime <= @to";
            sql += " GROUP BY v.Category";
            using var cmd  = new SqlCommand(sql, conn);
            if (from.HasValue) cmd.Parameters.AddWithValue("@from", from.Value);
            if (to.HasValue)   cmd.Parameters.AddWithValue("@to",   to.Value);
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        // ---- PRIVATE HELPERS ------------------------------------
        private static string GetJoinQuery(string whereClause) =>
            "SELECT rn.RentalID,rn.UserID,rn.VehicleID,rn.StartStationID,rn.EndStationID," +
            "rn.StartTime,rn.EndTime,rn.BaseFare,rn.DiscountApplied,rn.FinalFare,rn.PaymentMethod," +
            "u.Username,v.Category AS VehicleCategory," +
            "ss.StationName AS StartStationName, es.StationName AS EndStationName " +
            "FROM Rentals rn " +
            "JOIN Users u ON rn.UserID=u.UserID " +
            "JOIN Vehicles v ON rn.VehicleID=v.VehicleID " +
            "JOIN Stations ss ON rn.StartStationID=ss.StationID " +
            "LEFT JOIN Stations es ON rn.EndStationID=es.StationID " +
            $"{whereClause} ORDER BY rn.RentalID DESC";

        private static Rental MapRental(SqlDataReader r) => new()
        {
            RentalID        = (int)r["RentalID"],
            UserID          = (int)r["UserID"],
            VehicleID       = (int)r["VehicleID"],
            StartStationID  = (int)r["StartStationID"],
            EndStationID    = r["EndStationID"]   as int?,
            StartTime       = (DateTime)r["StartTime"],
            EndTime         = r["EndTime"]         as DateTime?,
            BaseFare        = r["BaseFare"]        as decimal?,
            DiscountApplied = (bool)r["DiscountApplied"],
            FinalFare       = r["FinalFare"]       as decimal?,
            PaymentMethod   = r["PaymentMethod"]   as string,
            Username        = (string)r["Username"],
            VehicleCategory = (string)r["VehicleCategory"],
            StartStationName= (string)r["StartStationName"],
            EndStationName  = r["EndStationName"]  as string ?? ""
        };
    }
}
