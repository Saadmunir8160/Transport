namespace AjkTransportSystem.API.DTOs
{
    public class TimingDto
    {
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int? DefaultDriverId { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string? OperatingDays { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? Status { get; set; }
    }
}
