namespace AjkTransportSystem.API.DTOs
{
    public class DriverDto
    {
        public int? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? IDNumber { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? AssignedVehicleId { get; set; }
        public string Status { get; set; } = "Active";
    }
}
