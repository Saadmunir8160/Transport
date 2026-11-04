namespace AjkTransportSystem.API.DTOs
{
    public class RouteStopDto
    {
        public int RouteStopId { get; set; }
        public int RouteId { get; set; }
        public string StopName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int DistanceFromStart { get; set; }
        public decimal FareFromStart { get; set; }
        public int? MinutesFromDeparture { get; set; }
    }
}
