namespace backend.Models;

public class RouteStop : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    
    public Guid RouteId { get; set; }
    public Route? Route { get; set; }
    
    public Guid StopId { get; set; }
    public Stop? Stop { get; set; }
    
    public int StopOrder { get; set; }
}
