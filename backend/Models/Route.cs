namespace backend.Models;

public class Route : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}
