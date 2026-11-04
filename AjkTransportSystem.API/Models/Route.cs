namespace AjkTransportSystem.API.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public decimal Distance { get; set; }
        public string? Status { get; set; } // Active, Inactive
        public string? Direction { get; set; }
        
        // Added for the new requirements
        public decimal? BaseFare { get; set; }
        public int? BaselineDuration { get; set; } // in minutes
        public bool? IsSeasonal { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();
    }
}
