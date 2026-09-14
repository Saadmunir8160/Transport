namespace backend.Services;

public interface ITenantProvider
{
    Guid? GetTenantId();
    void SetTenantId(Guid? tenantId);
}
