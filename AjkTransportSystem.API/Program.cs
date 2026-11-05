using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Port binding - Listen on 0.0.0.0:80 (or ASPNETCORE_HTTP_PORTS / ASPNETCORE_URLS / PORT)
var configuredUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
    ?? (Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS") != null 
        ? $"http://0.0.0.0:{Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")}"
        : null)
    ?? (Environment.GetEnvironmentVariable("PORT") != null 
        ? $"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT")}"
        : "http://0.0.0.0:80");

builder.WebHost.UseUrls(configuredUrls);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Configure CORS
var corsOriginsEnv = Environment.GetEnvironmentVariable("Cors__AllowedOrigins");
var allowVercelPreviews = (Environment.GetEnvironmentVariable("Cors__AllowVercelPreviews") ?? "false")
    .Equals("true", StringComparison.OrdinalIgnoreCase);

var allowedOrigins = (corsOriginsEnv ?? "https://transport-bleo.vercel.app,http://localhost:4200")
    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(origin => origin.TrimEnd('/'))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

        if (allowVercelPreviews)
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                try
                {
                    var uri = new Uri(origin);
                    return uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            });
        }
    });
});

// Read and validate JWT settings from environment variables
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || jwtKey.Contains("YOUR_"))
{
    var err = "FATAL: Jwt__Key environment variable is required and must be at least 32 characters long.";
    Console.WriteLine(err);
    throw new InvalidOperationException(err);
}

var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") 
    ?? builder.Configuration["Jwt:Issuer"] 
    ?? "AjkTransportSystem";

var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") 
    ?? builder.Configuration["Jwt:Audience"] 
    ?? "AjkTransportSystemUsers";

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

// Read and validate database connection string
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_"))
{
    var err = "FATAL: ConnectionStrings__DefaultConnection environment variable is required.";
    Console.WriteLine(err);
    throw new InvalidOperationException(err);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Forwarded Headers for reverse proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Health check endpoints (Return 200 OK without requiring authentication or DB connectivity)
app.MapGet("/", () => Results.Ok(new { status = "Online", service = "AJK Transport System API", timestamp = DateTime.UtcNow }));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// CORS middleware MUST come before UseAuthentication / UseAuthorization
app.UseCors("AllowConfiguredOrigins");

// Apply EF database migrations and seed users inside a scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying EF database migrations via db.Database.Migrate()...");
        context.Database.Migrate();
        logger.LogInformation("EF migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations: {FullException}", ex.ToString());
    }

    try
    {
        // Seed only if Users table is completely empty
        if (!context.Users.Any())
        {
            logger.LogInformation("Users table is empty. Checking environment seed passwords...");
            var adminPass = Environment.GetEnvironmentVariable("Seed__AdminPassword");
            var driverPass = Environment.GetEnvironmentVariable("Seed__DriverPassword");
            var passengerPass = Environment.GetEnvironmentVariable("Seed__PassengerPassword");

            var usersToSeed = new List<User>();

            if (!string.IsNullOrWhiteSpace(adminPass))
            {
                usersToSeed.Add(new User
                {
                    Name = "System Admin",
                    Email = "admin@ajk.com",
                    PasswordHash = AuthService.HashPassword(adminPass),
                    Role = "Admin",
                    Status = "Active",
                    OrganizationId = null
                });
            }
            else
            {
                logger.LogWarning("Seed__AdminPassword is not set. Skipping seed for admin user.");
            }

            if (!string.IsNullOrWhiteSpace(driverPass))
            {
                usersToSeed.Add(new User
                {
                    Name = "Demo Driver",
                    Email = "driver@ajk.com",
                    PasswordHash = AuthService.HashPassword(driverPass),
                    Role = "Driver",
                    Status = "Active",
                    OrganizationId = null
                });
            }
            else
            {
                logger.LogWarning("Seed__DriverPassword is not set. Skipping seed for driver@ajk.com.");
            }

            if (!string.IsNullOrWhiteSpace(passengerPass))
            {
                usersToSeed.Add(new User
                {
                    Name = "Test Passenger",
                    Email = "passenger@ajk.com",
                    PasswordHash = AuthService.HashPassword(passengerPass),
                    Role = "Passenger",
                    Status = "Active",
                    OrganizationId = null
                });
            }
            else
            {
                logger.LogWarning("Seed__PassengerPassword is not set. Skipping seed for passenger user.");
            }

            if (usersToSeed.Count > 0)
            {
                context.Users.AddRange(usersToSeed);
                context.SaveChanges();
                logger.LogInformation("Seeded {Count} initial user(s) into database.", usersToSeed.Count);
            }
        }
        else
        {
            logger.LogInformation("Users table already populated ({Count} users). Seed skipped.", context.Users.Count());
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding users: {FullException}", ex.ToString());
    }
}

// Swagger documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AJK Transport System API v1");
    c.RoutePrefix = "swagger";
});

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
