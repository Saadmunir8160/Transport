using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AjkTransportSystem.API.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log state-changing operations
            if (context.Request.Method == HttpMethods.Post ||
                context.Request.Method == HttpMethods.Put ||
                context.Request.Method == HttpMethods.Delete)
            {
                var path = context.Request.Path.Value;
                
                // Skip auth/login logs here if handled separately, or log them
                if (path != null && path.Contains("/api/auth/login"))
                {
                    await _next(context);
                    return;
                }

                // Call the next middleware in the pipeline first
                // To get the status code, we need to wait for it to finish.
                await _next(context);

                // If successful (or even if failed, we can log it)
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    // Create a scope to resolve Scoped AppDbContext
                    using var scope = context.RequestServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var user = context.User;
                    string action = $"{context.Request.Method} {path}";
                    string entityName = path.Split('/').LastOrDefault() ?? "Unknown";

                    int? orgId = null;
                    if (user?.Identity?.IsAuthenticated == true)
                    {
                        var orgClaim = user.Claims.FirstOrDefault(c => c.Type == "OrganizationId");
                        if (orgClaim != null && int.TryParse(orgClaim.Value, out int parsedOrg))
                        {
                            orgId = parsedOrg;
                        }
                    }

                    var auditLog = new AuditLog
                    {
                        Action = action,
                        EntityName = entityName,
                        EntityId = user?.Identity?.Name ?? "Anonymous",
                        Description = $"Endpoint {action} accessed successfully.",
                        Timestamp = DateTime.UtcNow,
                        OrganizationId = orgId
                    };

                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
