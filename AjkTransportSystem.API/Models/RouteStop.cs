namespace AjkTransportSystem.API.Models
{
    public class RouteStop
    {
        public int RouteStopId { get; set; }
        public int RouteId { get; set; }
        public string StopName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public int DistanceFromStart { get; set; } // km
        public decimal FareFromStart { get; set; } // Rs.
        public int? MinutesFromDeparture { get; set; } // minutes
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public Route? Route { get; set; }
    }
}
