namespace backend.Models;

public class Vehicle : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Bus, Minibus, Cab
    public int Capacity { get; set; }
    public string Model { get; set; } = string.Empty;
}
