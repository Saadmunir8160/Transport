namespace AjkTransportSystem.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; } // Admin, User, SuperAdmin, TransportAdmin
        
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        // Contact & Profile
        public string? Phone { get; set; }
        public string? LanguagePreference { get; set; } // Urdu, English, Hindi

        // Driver specific fields
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string? LicenseType { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public string? IDNumber { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? AssignedVehicleId { get; set; }

        public string? Status { get; set; } // Active, Inactive

        // OTP fields for Phone verification
        public string? PhoneOTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public int OTPAttempts { get; set; } = 0;
    }
}
