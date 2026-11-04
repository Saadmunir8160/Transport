using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AjkTransportSystem.API.Services
{
    public interface ITenantProvider
    {
        int? GetTenantId();
        bool IsSuperAdmin();
    }

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var orgIdClaim = user.Claims.FirstOrDefault(c => c.Type == "OrganizationId");
                if (orgIdClaim != null && int.TryParse(orgIdClaim.Value, out int orgId))
                {
                    return orgId;
                }
            }
            return null;
        }

        public bool IsSuperAdmin()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true && user.IsInRole("SuperAdmin");
        }
    }
}
