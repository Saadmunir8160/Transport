using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "file://")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AjkTransportSystem.API.Services.ITenantProvider, AjkTransportSystem.API.Services.TenantProvider>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create default users if not exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Ensure database is created/migrated
    context.Database.Migrate();

    if (!context.Users.Any(u => u.Role == "SuperAdmin"))
    {
        context.Users.Add(new User
        {
            Name = "Super Admin",
            Email = "superadmin@ajk.com",
            PasswordHash = AuthService.HashPassword("Admin@123"),
            Role = "SuperAdmin"
        });
    }

    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        context.Users.Add(new User
        {
            Name = "System Admin",
            Email = "admin@ajk.com",
            PasswordHash = AuthService.HashPassword("Admin@123"),
            Role = "Admin"
        });
    }

    if (!context.Users.Any(u => u.Role == "TransportAdmin"))
    {
        context.Users.Add(new User
        {
            Name = "Transport Admin",
            Email = "transportadmin@ajk.com",
            PasswordHash = AuthService.HashPassword("Transport@123"),
            Role = "TransportAdmin"
        });
    }

    if (!context.Users.Any(u => u.Role == "Passenger"))
    {
        context.Users.Add(new User
        {
            Name = "Test Passenger",
            Email = "passenger@ajk.com",
            PasswordHash = AuthService.HashPassword("Pass@123"),
            Role = "Passenger"
        });
    }

    context.SaveChanges();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AjkTransportSystem.API.Middleware.SessionValidationMiddleware>();
app.UseMiddleware<AjkTransportSystem.API.Middleware.AuditLoggingMiddleware>();
app.MapControllers();

app.Run();
