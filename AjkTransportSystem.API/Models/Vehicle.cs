namespace AjkTransportSystem.API.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string VehicleType { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? SeatLayout { get; set; } // e.g. "2x2", "2x1"
        public string? FuelType { get; set; }

        // Document Expiries directly on vehicle for easy querying
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? FitnessExpiry { get; set; }
        public DateTime? PermitExpiry { get; set; }
        public DateTime? PUCExpiry { get; set; }
        public DateTime? NextDocumentExpiry { get; set; }
    }
}
