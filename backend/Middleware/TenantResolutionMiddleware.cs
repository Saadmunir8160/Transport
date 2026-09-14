using backend.Services;

namespace backend.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var host = context.Request.Host.Host;
        string subdomain = host.Split('.')[0];

        // TODO: In a real scenario, look up the Tenant ID from the database using this subdomain.
        // For development/testing, we also check for an X-Tenant-ID header.
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader) && Guid.TryParse(tenantIdHeader, out var parsedId))
        {
            tenantProvider.SetTenantId(parsedId);
        }
        else if (subdomain.Equals("srinagar", StringComparison.OrdinalIgnoreCase))
        {
             // Mock ID for "srinagar" subdomain testing
             tenantProvider.SetTenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        }

        await _next(context);
    }
}
