namespace backend.Models.DTOs;

public class TripDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateTripDto
{
    public Guid RouteId { get; set; }
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
}

public class UpdateTripStatusDto
{
    public string Status { get; set; } = string.Empty;
}
