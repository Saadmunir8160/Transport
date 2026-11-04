namespace AjkTransportSystem.API.Models
{
    public class Timing
    {
        public int TimingId { get; set; }
        public int RouteId { get; set; }
        public int? VehicleId { get; set; } // Default vehicle (optional)
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string? OperatingDays { get; set; } // e.g. "Mon,Tue,Wed"
        public string? Status { get; set; } // Active, Paused
        
        public int? DefaultDriverId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        
        public Route? Route { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
