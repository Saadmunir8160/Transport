namespace AjkTransportSystem.API.DTOs
{
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RouteId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
    }
}
