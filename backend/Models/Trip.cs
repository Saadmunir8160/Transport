namespace backend.Models;

public class Trip : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    
    public Guid RouteId { get; set; }
    public Route? Route { get; set; }
    
    public Guid DriverId { get; set; }
    public Driver? Driver { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public DateTime DepartureTime { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    
    public decimal Price { get; set; }
    
    public string Status { get; set; } = "Scheduled"; // Scheduled, Active, Completed, Cancelled
}
