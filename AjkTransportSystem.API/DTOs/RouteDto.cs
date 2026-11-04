namespace AjkTransportSystem.API.DTOs
{
    public class RouteDto
    {
        public string RouteName { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public decimal Distance { get; set; }
        public string? Direction { get; set; }
        public decimal? BaseFare { get; set; }
        public int? BaselineDuration { get; set; }
        public bool? IsSeasonal { get; set; }
        public string? Status { get; set; }
    }
}
