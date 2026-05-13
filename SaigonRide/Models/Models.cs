namespace SaigonRide.Models
{
    // -------------------------------------------------------
    // VEHICLE — maps to Vehicles table
    // -------------------------------------------------------
    public class Vehicle
    {
        public int    VehicleID   { get; set; }
        public string Category    { get; set; } = "StandardBike"; // StandardBike | EScooter
        public string Status      { get; set; } = "Available";    // Available | InTransit | Maintenance
        public int?   StationID   { get; set; }
        public string StationName { get; set; } = ""; // joined display field

        /// <summary>Fare rate in VND per minute (FR3).</summary>
        public int RatePerMinute => Category == "EScooter" ? 1500 : 500;

        public string DisplayCategory => Category == "EScooter" ? "E-Scooter" : "Standard Bike";
    }

    // -------------------------------------------------------
    // STATION — maps to Stations table
    // -------------------------------------------------------
    public class Station
    {
        public int    StationID    { get; set; }
        public string StationName  { get; set; } = "";
        public int    Capacity     { get; set; }
        public int    CurrentCount { get; set; }

        /// <summary>Occupancy percentage (0–100).</summary>
        public double OccupancyPercent =>
            Capacity > 0 ? (double)CurrentCount / Capacity * 100 : 0;

        /// <summary>True if station is "Low Inventory" — triggers 15% discount (FR2).</summary>
        public bool IsLowInventory => OccupancyPercent < 20;
    }

    // -------------------------------------------------------
    // RENTAL — maps to Rentals table
    // -------------------------------------------------------
    public class Rental
    {
        public int       RentalID        { get; set; }
        public int       UserID          { get; set; }
        public int       VehicleID       { get; set; }
        public int       StartStationID  { get; set; }
        public int?      EndStationID    { get; set; }
        public DateTime  StartTime       { get; set; }
        public DateTime? EndTime         { get; set; }
        public decimal?  BaseFare        { get; set; }
        public bool      DiscountApplied { get; set; }
        public decimal?  FinalFare       { get; set; }
        public string?   PaymentMethod   { get; set; }

        // Joined display fields
        public string Username          { get; set; } = "";
        public string VehicleCategory   { get; set; } = "";
        public string StartStationName  { get; set; } = "";
        public string EndStationName    { get; set; } = "";

        /// <summary>Duration in whole minutes (null if rental still active).</summary>
        public int? DurationMinutes =>
            EndTime.HasValue
                ? (int)Math.Max(1, Math.Ceiling((EndTime.Value - StartTime).TotalMinutes))
                : null;

        public bool IsActive => !EndTime.HasValue;
    }
}
