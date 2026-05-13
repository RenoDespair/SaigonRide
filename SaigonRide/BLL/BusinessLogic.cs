using System.Security.Cryptography;
using System.Text;
using SaigonRide.DAL;
using SaigonRide.Models;
using System.Data;

namespace SaigonRide.BLL
{
    // ============================================================
    // USER BLL — authentication & registration
    // ============================================================
    public class UserBLL
    {
        private readonly UserDAL _dal = new();

        /// <summary>SHA-256 hash (NFR: Security).</summary>
        public static string HashPassword(string plain)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes);  // uppercase hex
        }

        public (User? user, string error) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (null, "Username and password are required.");
            string hash = HashPassword(password);
            var user = _dal.GetByCredentials(username, hash);
            return user != null
                ? (user, "")
                : (null, "Invalid username or password.");
        }

        public (bool ok, string error) Register(string username, string password,
                                                 string userType, string? passportId)
        {
            if (string.IsNullOrWhiteSpace(username))   return (false, "Username is required.");
            if (password.Length < 6)                    return (false, "Password must be at least 6 characters.");
            if (userType == "Tourist" && string.IsNullOrWhiteSpace(passportId))
                return (false, "Passport ID is required for Foreign Tourists.");
            if (_dal.UsernameExists(username))          return (false, "Username already taken.");

            var user = new User
            {
                Username     = username,
                PasswordHash = HashPassword(password),
                UserType     = userType,
                PassportID   = passportId
            };
            return _dal.Insert(user)
                ? (true, "Registration successful.")
                : (false, "Registration failed. Please try again.");
        }
    }

    // ============================================================
    // VEHICLE BLL — UC05 (Hieu)
    // ============================================================
    public class VehicleBLL
    {
        private readonly VehicleDAL _dal = new();

        public List<Vehicle> GetAll()           => _dal.GetAll();
        public List<Vehicle> Search(string? cat, string? stat) => _dal.Search(cat, stat);
        public List<Vehicle> GetAvailableByStation(int stId)   => _dal.GetAvailableByStation(stId);

        public (bool ok, string msg) Add(string category, string status, int? stationId)
        {
            if (string.IsNullOrWhiteSpace(category)) return (false, "Category is required.");
            if (string.IsNullOrWhiteSpace(status))   return (false, "Status is required.");
            var v = new Vehicle { Category = category, Status = status, StationID = stationId };
            return _dal.Insert(v)
                ? (true,  "Vehicle added successfully.")
                : (false, "Insert failed.");
        }

        public (bool ok, string msg) Edit(int id, string category, string status, int? stationId)
        {
            if (string.IsNullOrWhiteSpace(category)) return (false, "Category is required.");
            if (string.IsNullOrWhiteSpace(status))   return (false, "Status is required.");
            var v = new Vehicle { VehicleID = id, Category = category, Status = status, StationID = stationId };
            return _dal.Update(v)
                ? (true,  "Vehicle updated successfully.")
                : (false, "Update failed.");
        }

        public (bool ok, string msg) Remove(int vehicleId) => _dal.Delete(vehicleId);
    }

    // ============================================================
    // RENTAL BLL — UC04 (Bao)
    // ============================================================
    public class RentalBLL
    {
        private readonly RentalDAL  _rentalDal  = new();
        private readonly VehicleDAL _vehicleDal = new();
        private readonly StationDAL _stationDal = new();

        public List<Rental> GetAll()               => _rentalDal.GetAll();
        public List<Rental> GetByUser(int userId)  => _rentalDal.GetByUser(userId);
        public Rental?      GetActive(int userId)  => _rentalDal.GetActiveByUser(userId);

        /// <summary>Start rental — validates pre-conditions (NFR: Reliability).</summary>
        public (bool ok, string msg, int rentalId) StartRental(int userId, int vehicleId, int stationId)
        {
            // E1: Already has active rental
            if (_rentalDal.GetActiveByUser(userId) != null)
                return (false, "You already have an active rental.", 0);

            // E1: Vehicle must be Available
            var v = _vehicleDal.GetById(vehicleId);
            if (v == null || v.Status != "Available")
                return (false, "Selected vehicle is not available.", 0);

            var rn = new Rental
            {
                UserID         = userId,
                VehicleID      = vehicleId,
                StartStationID = stationId,
                StartTime      = DateTime.Now
            };
            int id = _rentalDal.Insert(rn);
            if (id <= 0) return (false, "Failed to start rental.", 0);

            _vehicleDal.UpdateStatus(vehicleId, "InTransit", null);
            _stationDal.RefreshCount(stationId);
            return (true, "Rental started.", id);
        }

        /// <summary>
        /// Calculate fare — returns breakdown for checkout screen.
        /// Core business rule (FR3, FR4).
        /// </summary>
        public (decimal baseFare, bool discount, decimal finalFare, int minutes)
            CalculateFare(int rentalId, int returnStationId)
        {
            var rn = _rentalDal.GetById(rentalId)
                ?? throw new Exception("Rental not found.");
            var v  = _vehicleDal.GetById(rn.VehicleID)
                ?? throw new Exception("Vehicle not found.");
            var st = _stationDal.GetById(returnStationId)
                ?? throw new Exception("Station not found.");

            int minutes  = (int)Math.Max(1, Math.Ceiling((DateTime.Now - rn.StartTime).TotalMinutes));
            decimal base_ = minutes * v.RatePerMinute;
            bool disc     = st.IsLowInventory;          // < 20% capacity
            decimal final = disc ? base_ * 0.85m : base_;
            return (base_, disc, Math.Round(final, 0), minutes);
        }

        /// <summary>Complete rental and record payment.</summary>
        public (bool ok, string msg) CompleteRental(int rentalId, int returnStationId,
                                                     decimal baseFare, bool discount,
                                                     decimal finalFare, string paymentMethod)
        {
            var rn = _rentalDal.GetById(rentalId);
            if (rn == null) return (false, "Rental not found.");

            bool ok = _rentalDal.Complete(rentalId, returnStationId, DateTime.Now,
                                          baseFare, discount, finalFare, paymentMethod);
            if (!ok) return (false, "Failed to complete rental.");

            _vehicleDal.UpdateStatus(rn.VehicleID, "Available", returnStationId);
            _stationDal.RefreshCount(returnStationId);
            if (rn.StartStationID != returnStationId)
                _stationDal.RefreshCount(rn.StartStationID);
            return (true, "Payment complete. Rental ended.");
        }

        public (bool ok, string msg) AdminUpdate(int rentalId, int endStationId, string pm)
            => _rentalDal.UpdateAdmin(rentalId, endStationId, pm)
                ? (true,  "Rental updated.")
                : (false, "Update failed.");

        public (bool ok, string msg) AdminDelete(int rentalId)
        {
            var rn = _rentalDal.GetById(rentalId);
            if (rn == null) return (false, "Rental not found.");
            if (rn.IsActive) return (false, "Cannot delete an active rental.");
            return _rentalDal.Delete(rentalId)
                ? (true,  "Rental deleted.")
                : (false, "Delete failed.");
        }
    }

    // ============================================================
    // REPORT BLL — wraps report queries
    // ============================================================
    public class ReportBLL
    {
        private readonly RentalDAL  _rentalDal  = new();
        private readonly StationDAL _stationDal = new();

        /// <summary>Revenue report by vehicle category (Hieu — UC06).</summary>
        public DataTable GetRevenueReport(DateTime? from, DateTime? to)
            => _rentalDal.GetRevenueByCategory(from, to);

        /// <summary>Station inventory/utilization report (Bao — UC07).</summary>
        public List<Station> GetStationInventoryReport()
            => _stationDal.GetAll();
    }
}
