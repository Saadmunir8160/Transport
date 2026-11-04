namespace AjkTransportSystem.API.DTOs
{
    public class AdvisoryDto
    {
        public string RoadOrSegment { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime StartedAt { get; set; }
        public DateTime? ExpectedReopening { get; set; }
        public string AffectedRoutes { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
