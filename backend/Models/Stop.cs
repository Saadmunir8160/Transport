namespace backend.Models;

public class Stop : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
