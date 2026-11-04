namespace AjkTransportSystem.API.DTOs
{
    public class TripDto
    {
        public int RouteId { get; set; }
        public int? TimingId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime TravelDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
    }
}
