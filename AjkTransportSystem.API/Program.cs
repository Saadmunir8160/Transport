using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Port binding for Render / Railway / Cloud hosts (respect PORT environment variable)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5242";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Configure CORS
var defaultOrigins = new[] { "https://transport-bleo.vercel.app", "http://localhost:4200" };
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? defaultOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(configuredOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    jwtKey = "SuperSecureTransportSystemKey2026!#JWT_AJK_PRODUCTION";
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AjkTransportSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AjkTransportSystemUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AjkTransportSystem.API.Services.ITenantProvider, AjkTransportSystem.API.Services.TenantProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Forwarded Headers for reverse proxies (Render, Railway, Nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Automatically apply database migrations and seed default demo users on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Automatically apply any pending EF Core migrations
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }

    var demoPassword = builder.Configuration["Seed:DemoPassword"];
    if (string.IsNullOrWhiteSpace(demoPassword) || demoPassword.Length < 10)
    {
        demoPassword = "Transport@Pass2026";
    }
    
    var passwordHash = AuthService.HashPassword(demoPassword);

        // Ensure demo users exist or have their passwords synced
        void UpsertUser(string email, string name, string role)
        {
            var existing = context.Users.FirstOrDefault(u => u.Email == email);
            if (existing == null)
            {
                context.Users.Add(new User
                {
                    Name = name,
                    Email = email,
                    PasswordHash = passwordHash,
                    Role = role
                });
            }
            else
            {
                existing.PasswordHash = passwordHash;
            }
        }

        UpsertUser("superadmin@ajk.com", "Super Admin", "SuperAdmin");
        UpsertUser("admin@ajk.com", "System Admin", "Admin");
        UpsertUser("transportadmin@ajk.com", "Transport Admin", "TransportAdmin");
        UpsertUser("driver@ajk.com", "Demo Driver", "Driver");
        UpsertUser("passenger@ajk.com", "Test Passenger", "Passenger");

        try
        {
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding demo users.");
        }
    }

// Swagger enabled in production too for live demo/testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AJK Transport System API v1");
    c.RoutePrefix = "swagger";
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.UseCors("AllowConfiguredOrigins");

// Respect forwarded proto behind HTTPS reverse proxy
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto) && proto == "https")
    {
        context.Request.Scheme = "https";
    }
    return next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AjkTransportSystem.API.Middleware.SessionValidationMiddleware>();
app.UseMiddleware<AjkTransportSystem.API.Middleware.AuditLoggingMiddleware>();
app.MapControllers();
app.MapHub<LiveTrackingHub>("/hubs/tracking");

app.Run();
