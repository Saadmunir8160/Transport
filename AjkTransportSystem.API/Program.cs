    }

    try
    {
        // Seed only if Users table is completely empty.
        if (!context.Users.Any())
        {
            logger.LogInformation("Users table is empty. Checking configured seed passwords.");

            var adminPass = builder.Configuration["Seed:AdminPassword"];
            var driverPass = builder.Configuration["Seed:DriverPassword"];
            var passengerPass = builder.Configuration["Seed:PassengerPassword"];

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
                logger.LogWarning("Seed:AdminPassword is not set. Skipping seed for admin user.");
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
                logger.LogWarning("Seed:DriverPassword is not set. Skipping seed for driver@ajk.com.");
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
                logger.LogWarning("Seed:PassengerPassword is not set. Skipping seed for passenger@ajk.com.");
            }

            if (usersToSeed.Count > 0)
            {
                context.Users.AddRange(usersToSeed);
                context.SaveChanges();

                logger.LogInformation(
                    "Seeded {Count} initial user(s) into database.",
                    usersToSeed.Count);
            }
        }
        else
        {
            logger.LogInformation(
                "Users table already populated ({Count} users). Seed skipped.",
                context.Users.Count());
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding users: {FullException}", ex.ToString());
    }
}

// Swagger documentation.
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AJK Transport System API v1");
    c.RoutePrefix = "swagger";
});

// Respect forwarded protocol behind HTTPS reverse proxy.
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto) &&
        proto == "https")
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
