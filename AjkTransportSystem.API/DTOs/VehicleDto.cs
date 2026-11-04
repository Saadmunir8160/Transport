namespace AjkTransportSystem.API.DTOs
{
    public class VehicleDto
    {
        public int? VehicleId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = "Active";
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? SeatLayout { get; set; }
        public string? FuelType { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? FitnessExpiry { get; set; }
        public DateTime? PermitExpiry { get; set; }
        public DateTime? PUCExpiry { get; set; }
    }
}
