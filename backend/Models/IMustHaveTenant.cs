namespace backend.Models;

public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}
