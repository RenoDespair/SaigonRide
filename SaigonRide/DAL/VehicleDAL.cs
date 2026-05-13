using System.Data.SqlClient;
using SaigonRide.Database;
using SaigonRide.Models;

namespace SaigonRide.DAL
{
    /// <summary>DAL for Vehicles — owned by Vu Van Minh Hieu (UC05).</summary>
    public class VehicleDAL
    {
        // ---- READ ------------------------------------------------
        public List<Vehicle> GetAll()
        {
            var list = new List<Vehicle>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT v.VehicleID,v.Category,v.Status,v.StationID,s.StationName " +
                "FROM Vehicles v LEFT JOIN Stations s ON v.StationID=s.StationID " +
                "ORDER BY v.VehicleID", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapVehicle(r));
            return list;
        }

        public List<Vehicle> Search(string? category, string? status)
        {
            var list = new List<Vehicle>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var sql =
                "SELECT v.VehicleID,v.Category,v.Status,v.StationID,s.StationName " +
                "FROM Vehicles v LEFT JOIN Stations s ON v.StationID=s.StationID WHERE 1=1";
            if (!string.IsNullOrEmpty(category)) sql += " AND v.Category=@cat";
            if (!string.IsNullOrEmpty(status))   sql += " AND v.Status=@stat";
            sql += " ORDER BY v.VehicleID";
            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(category)) cmd.Parameters.AddWithValue("@cat", category);
            if (!string.IsNullOrEmpty(status))   cmd.Parameters.AddWithValue("@stat", status);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapVehicle(r));
            return list;
        }

        public List<Vehicle> GetAvailableByStation(int stationId)
        {
            var list = new List<Vehicle>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT v.VehicleID,v.Category,v.Status,v.StationID,s.StationName " +
                "FROM Vehicles v LEFT JOIN Stations s ON v.StationID=s.StationID " +
                "WHERE v.StationID=@sid AND v.Status='Available'", conn);
            cmd.Parameters.AddWithValue("@sid", stationId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapVehicle(r));
            return list;
        }

        public Vehicle? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT v.VehicleID,v.Category,v.Status,v.StationID,s.StationName " +
                "FROM Vehicles v LEFT JOIN Stations s ON v.StationID=s.StationID " +
                "WHERE v.VehicleID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapVehicle(r) : null;
        }

        // ---- CREATE ----------------------------------------------
        public bool Insert(Vehicle v)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Vehicles(Category,Status,StationID) VALUES(@cat,@stat,@sid)", conn);
            cmd.Parameters.AddWithValue("@cat",  v.Category);
            cmd.Parameters.AddWithValue("@stat", v.Status);
            cmd.Parameters.AddWithValue("@sid",  (object?)v.StationID ?? DBNull.Value);
            int rows = cmd.ExecuteNonQuery();
            if (rows > 0 && v.StationID.HasValue && v.Status == "Available")
                AdjustStationCount(conn, v.StationID.Value, +1);
            return rows > 0;
        }

        // ---- UPDATE ----------------------------------------------
        public bool Update(Vehicle v)
        {
            var old = GetById(v.VehicleID);
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Vehicles SET Category=@cat,Status=@stat,StationID=@sid " +
                "WHERE VehicleID=@id", conn);
            cmd.Parameters.AddWithValue("@cat",  v.Category);
            cmd.Parameters.AddWithValue("@stat", v.Status);
            cmd.Parameters.AddWithValue("@sid",  (object?)v.StationID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id",   v.VehicleID);
            int rows = cmd.ExecuteNonQuery();
            // Adjust station counts if station or status changed
            if (rows > 0 && old != null)
            {
                if (old.StationID.HasValue && old.Status == "Available")
                    AdjustStationCount(conn, old.StationID.Value, -1);
                if (v.StationID.HasValue && v.Status == "Available")
                    AdjustStationCount(conn, v.StationID.Value, +1);
            }
            return rows > 0;
        }

        public bool UpdateStatus(int vehicleId, string status, int? stationId = null)
        {
            var v = GetById(vehicleId);
            if (v == null) return false;
            v.Status    = status;
            v.StationID = stationId ?? v.StationID;
            return Update(v);
        }

        // ---- DELETE ----------------------------------------------
        /// <summary>Returns false if vehicle is InTransit (E1 exception flow).</summary>
        public (bool success, string message) Delete(int vehicleId)
        {
            var v = GetById(vehicleId);
            if (v == null) return (false, "Vehicle not found.");
            if (v.Status == "InTransit")
                return (false, "Cannot delete a vehicle currently in use.");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM Vehicles WHERE VehicleID=@id", conn);
            cmd.Parameters.AddWithValue("@id", vehicleId);
            int rows = cmd.ExecuteNonQuery();
            if (rows > 0 && v.StationID.HasValue && v.Status == "Available")
                AdjustStationCount(conn, v.StationID.Value, -1);
            return rows > 0
                ? (true,  "Vehicle deleted successfully.")
                : (false, "Delete failed.");
        }

        // ---- PRIVATE HELPERS ------------------------------------
        private static void AdjustStationCount(SqlConnection conn, int stationId, int delta)
        {
            using var cmd = new SqlCommand(
                "UPDATE Stations SET CurrentCount = CASE " +
                "WHEN CurrentCount + @d < 0 THEN 0 " +
                "WHEN CurrentCount + @d > Capacity THEN Capacity " +
                "ELSE CurrentCount + @d END WHERE StationID=@sid", conn);
            cmd.Parameters.AddWithValue("@d",   delta);
            cmd.Parameters.AddWithValue("@sid", stationId);
            cmd.ExecuteNonQuery();
        }

        private static Vehicle MapVehicle(SqlDataReader r) => new()
        {
            VehicleID   = (int)r["VehicleID"],
            Category    = (string)r["Category"],
            Status      = (string)r["Status"],
            StationID   = r["StationID"]   as int?,
            StationName = r["StationName"] as string ?? ""
        };
    }
}
