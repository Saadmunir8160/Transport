namespace AjkTransportSystem.API.Models
{
    public class Advisory
    {
        public int AdvisoryId { get; set; }
        public string RoadOrSegment { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty; // landslide, snow, flood, other
        public string Status { get; set; } = "Active"; // Active, Closed
        public DateTime StartedAt { get; set; }
        public DateTime? ExpectedReopening { get; set; }
        public string AffectedRoutes { get; set; } = string.Empty; // comma-separated route IDs
        public string Source { get; set; } = string.Empty;
        public string? AlternativeAction { get; set; }
        
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
