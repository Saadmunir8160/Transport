using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AjkTransportSystem.API.Data;

namespace AjkTransportSystem.API.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sessionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "SessionId");
                
                if (sessionIdClaim != null && int.TryParse(sessionIdClaim.Value, out int sessionId))
                {
                    // Resolve scoped DbContext
                    using var scope = context.RequestServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var session = await dbContext.UserSessions.FindAsync(sessionId);

                    if (session == null || session.IsRevoked)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Session is invalid or has been revoked.");
                        return;
                    }

                    // Optional: Update LastActivity
                    session.LastActivity = System.DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    // Missing SessionId claim, but authenticated. 
                    // To maintain backward compatibility with old tokens, we can let it pass, 
                    // or strictly reject. For security, we should reject or at least flag it.
                    // For now, let's reject since we want strict session control.
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Missing session identifier.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
