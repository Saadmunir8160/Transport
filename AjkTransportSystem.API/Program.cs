using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Port binding - Read from ASPNETCORE_URLS or PORT env var, default to port 80
var aspnetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(aspnetUrls))
{
    builder.WebHost.UseUrls(aspnetUrls);
}
else
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Configure CORS - Read allowed origins from Cors__AllowedOrigins env or Cors:AllowedOrigins in configuration
var corsOriginsEnv = Environment.GetEnvironmentVariable("Cors__AllowedOrigins");
string[] allowedOrigins;
if (!string.IsNullOrWhiteSpace(corsOriginsEnv))
{
    allowedOrigins = corsOriginsEnv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
else
{
    var defaultOrigins = new[] { "https://transport-bleo.vercel.app", "http://localhost:4200" };
    allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? defaultOrigins;
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Read JWT settings from environment variables with config fallback
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") 
    ?? builder.Configuration["Jwt:Key"] 
    ?? "SuperSecureTransportSystemKey2026!#JWT_AJK_PRODUCTION";

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

// Read Connection string from environment variable with fallback
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=sql5106.site4now.net;Database=db_ad0c00_ajkdb;User Id=db_ad0c00_ajkdb_admin;Password=Fs9R8EL$RP65fFN;TrustServerCertificate=True;Encrypt=False;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

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

// Apply EF migrations automatically at startup inside a scope, and seed default users only if the Users table is empty
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying EF database migrations...");
        context.Database.Migrate();
        logger.LogInformation("EF migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database: {Message}", ex.Message);
    }

    try
    {
        // Seed default users only if the Users table is empty
        if (!context.Users.Any())
        {
            logger.LogInformation("Users table is empty. Seeding default role users...");
            var defaultPassword = builder.Configuration["Seed:DemoPassword"] ?? "Transport@Pass2026";
            var passwordHash = AuthService.HashPassword(defaultPassword);

            context.Users.AddRange(
                new User
                {
                    Name = "Super Admin",
                    Email = "superadmin@ajk.com",
                    PasswordHash = passwordHash,
                    Role = "SuperAdmin",
                    Status = "Active"
                },
                new User
                {
                    Name = "System Admin",
                    Email = "admin@ajk.com",
                    PasswordHash = passwordHash,
                    Role = "Admin",
                    Status = "Active"
                },
                new User
                {
                    Name = "Transport Admin",
                    Email = "transportadmin@ajk.com",
                    PasswordHash = passwordHash,
                    Role = "TransportAdmin",
                    Status = "Active"
                },
                new User
                {
                    Name = "Demo Driver",
                    Email = "driver@ajk.com",
                    PasswordHash = passwordHash,
                    Role = "Driver",
                    Status = "Active"
                },
                new User
                {
                    Name = "Test Passenger",
                    Email = "passenger@ajk.com",
                    PasswordHash = passwordHash,
                    Role = "Passenger",
                    Status = "Active"
                }
            );

            context.SaveChanges();
            logger.LogInformation("Default users seeded successfully.");
        }
        else
        {
            logger.LogInformation("Users table already contains users. Skipping seed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding default users: {Message}", ex.Message);
    }
}

// Swagger enabled in production too for live demo/testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AJK Transport System API v1");
    c.RoutePrefix = "swagger";
});

// Root health check & API status endpoint for Cloud load balancers / Back4app
app.MapGet("/", () => Results.Ok(new { status = "Online", service = "AJK Transport System API", timestamp = DateTime.UtcNow }));

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// CORS middleware MUST be placed before UseAuthentication and UseAuthorization
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
