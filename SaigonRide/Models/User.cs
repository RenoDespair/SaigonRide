namespace SaigonRide.Models
{
    // Base user — maps to Users table
    public class User
    {
        public int    UserID       { get; set; }
        public string Username     { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string UserType     { get; set; } = "Local"; // Local | Tourist | Admin
        public string? PassportID  { get; set; }

        public bool IsAdmin   => UserType == "Admin";
        public bool IsLocal   => UserType == "Local";
        public bool IsTourist => UserType == "Tourist";

        /// <summary>Payment methods available to this user type.</summary>
        public string[] PaymentMethods => UserType switch
        {
            "Local"   => new[] { "MoMo", "VNPay", "Cash" },
            "Tourist" => new[] { "Apple Pay", "PayPal", "Cash" },
            _         => new[] { "Cash" }
        };
    }
}
