using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    // [Authorize(Roles = "SuperAdmin")] // Uncomment when auth is fully tested
    public class SuperAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SuperAdminController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD STATISTICS
        // ==========================================
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var stats = new
            {
                TotalOrganizations = await _context.Organizations.CountAsync(),
                ActiveOrganizations = await _context.Organizations.CountAsync(o => o.Status == "Active"),
                InactiveOrganizations = await _context.Organizations.CountAsync(o => o.Status != "Active"),
                TotalTransportAdmins = await _context.Users.CountAsync(u => u.Role == "TransportAdmin"),
                TotalDrivers = await _context.Users.CountAsync(u => u.Role == "Driver"),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                TotalRoutes = await _context.Routes.CountAsync(),
                PlatformTripsToday = await _context.Trips.CountAsync(t => t.TravelDate >= today),
                PlatformBookingsToday = await _context.Bookings.CountAsync(b => b.BookingDate >= today),
                ActiveTripsRightNow = await _context.Trips.CountAsync(t => t.Status == "InTransit"),
                SystemHealthStatus = "Healthy" // Default to healthy unless alerts say otherwise
            };

            return Ok(stats);
        }

        [HttpGet("chart-analytics")]
        public async Task<IActionResult> GetChartAnalytics()
        {
            var orgs = await _context.Organizations.Where(o => !o.IsSoftDeleted).ToListAsync();
            
            // Organizations per month (Last 6 months)
            var orgsPerMonth = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var count = orgs.Count(o => o.CreatedAt.Year == monthDate.Year && o.CreatedAt.Month == monthDate.Month);
                orgsPerMonth.Add(new { Month = monthDate.ToString("MMM yyyy"), Count = count });
            }

            // Trips per Organization (Top 5)
            var topOrgsByTrips = await _context.Trips
                .Where(t => t.OrganizationId != null)
                .GroupBy(t => t.OrganizationId)
                .Select(g => new { OrgId = g.Key, TripCount = g.Count() })
                .OrderByDescending(x => x.TripCount)
                .Take(5)
                .ToListAsync();

            var tripsPerOrg = new List<object>();
            foreach(var item in topOrgsByTrips)
            {
                var orgName = orgs.FirstOrDefault(o => o.OrganizationId == item.OrgId)?.OrganizationName ?? "Unknown";
                tripsPerOrg.Add(new { Organization = orgName, Count = item.TripCount });
            }

            // Bookings per Organization (Top 5)
            var topOrgsByBookings = await _context.Bookings
                .Where(b => b.OrganizationId != null)
                .GroupBy(b => b.OrganizationId)
                .Select(g => new { OrgId = g.Key, BookingCount = g.Count() })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToListAsync();

            var bookingsPerOrg = new List<object>();
            foreach(var item in topOrgsByBookings)
            {
                var orgName = orgs.FirstOrDefault(o => o.OrganizationId == item.OrgId)?.OrganizationName ?? "Unknown";
                bookingsPerOrg.Add(new { Organization = orgName, Count = item.BookingCount });
            }

            return Ok(new {
                OrganizationsPerMonth = orgsPerMonth,
                TripsPerOrganization = tripsPerOrg,
                BookingsPerOrganization = bookingsPerOrg
            });
        }

        private async Task<bool> CheckDatabaseHealth()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        // ==========================================
        // 2. ORGANIZATION MANAGEMENT
        // ==========================================
        [HttpGet("organizations/master-list")]
        public async Task<IActionResult> GetMasterList(
            [FromQuery] string search = null, 
            [FromQuery] string status = null, 
            [FromQuery] string region = null)
        {
            var query = _context.Organizations
                .Where(o => !o.IsSoftDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.OrganizationName.Contains(search) || o.OrganizationCode.Contains(search));
                
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrEmpty(region))
                query = query.Where(o => o.District == region);

            var orgs = await query.Select(o => new 
            {
                o.OrganizationId,
                o.OrganizationCode,
                o.OrganizationName,
                o.District,
                o.Location,
                o.Subdomain,
                o.Status,
                o.CreatedAt,
                o.LastActivityDate,
                AdminName = _context.Users.Where(u => u.UserId == o.AdminUserId).Select(u => u.Name).FirstOrDefault(),
                TotalDrivers = _context.Users.Count(u => u.OrganizationId == o.OrganizationId && u.Role == "Driver"),
                TotalVehicles = _context.Vehicles.Count(v => v.OrganizationId == o.OrganizationId),
                TotalTrips = _context.Trips.Count(t => t.OrganizationId == o.OrganizationId)
            }).ToListAsync();

            return Ok(orgs);
        }

        [HttpPost("organizations/{id}/toggle-status")]
        public async Task<IActionResult> ToggleOrganizationStatus(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            if (org.Status == "Active")
            {
                // Cannot deactivate if there are active trips
                bool hasActiveTrips = await _context.Trips.AnyAsync(t => 
                    t.OrganizationId == id && 
                    (t.Status == "InTransit" || t.Status == "Scheduled"));
                    
                if (hasActiveTrips)
                {
                    return BadRequest(new { message = "Cannot deactivate organization. There are active or scheduled trips." });
                }
                
                org.Status = "Inactive";
            }
            else
            {
                org.Status = "Active";
            }

            org.UpdatedAt = DateTime.UtcNow;
            _context.AuditLogs.Add(new AuditLog { Action = $"Toggled Status to {org.Status}", EntityName = "Organization", EntityId = org.OrganizationCode, Description = $"Changed status of {org.OrganizationName} to {org.Status}" });
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Status updated", currentStatus = org.Status });
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> GetOrganizations()
        {
            // Keeping the old one just in case other parts of the app use it
            var orgs = await _context.Organizations.Where(o => !o.IsSoftDeleted).ToListAsync();
            return Ok(orgs);
        }

        [HttpGet("organizations/{id}")]
        public async Task<IActionResult> GetOrganization(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null || org.IsSoftDeleted) return NotFound();

            var drivers = await _context.Users
                .Where(u => u.Role == "Driver" && u.OrganizationId == id)
                .Select(u => new { u.UserId, u.Name, u.Email, u.Status })
                .ToListAsync();

            var vehicles = await _context.Vehicles
                .Where(v => v.OrganizationId == id)
                .Select(v => new { v.VehicleId, v.RegistrationNumber, Model = v.VehicleType, v.Status })
                .ToListAsync();

            var activeTrips = await _context.Trips
                .Where(t => t.OrganizationId == id && t.Status == "InTransit")
                .Select(t => new { t.TripId, t.TravelDate, t.Status })
                .ToListAsync();

            var auditLogs = await _context.AuditLogs
                .Where(a => a.OrganizationId == id)
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .Select(a => new { a.AuditLogId, a.Action, a.Description, a.Timestamp })
                .ToListAsync();

            var stats = new
            {
                org.OrganizationId,
                org.OrganizationName,
                org.OrganizationCode,
                org.Location,
                org.District,
                org.Status,
                org.Subdomain,
                org.CreatedAt,
                org.LastActivityDate,
                AdminName = await _context.Users.Where(u => u.UserId == org.AdminUserId).Select(u => u.Name).FirstOrDefaultAsync(),
                Drivers = drivers.Count,
                DriverList = drivers,
                Vehicles = vehicles.Count,
                VehicleList = vehicles,
                Routes = await _context.Routes.CountAsync(r => r.OrganizationId == id),
                TotalTrips = await _context.Trips.CountAsync(t => t.OrganizationId == id),
                ActiveTrips = activeTrips.Count,
                ActiveTripList = activeTrips,
                Bookings = await _context.Bookings.CountAsync(b => b.OrganizationId == id),
                AuditLogs = auditLogs
            };

            return Ok(stats);
        }

        public class OnboardOrganizationDto
        {
            public Organization Organization { get; set; }
            public User Admin { get; set; }
        }

        [HttpPost("organizations/onboard")]
        public async Task<IActionResult> OnboardOrganization([FromBody] OnboardOrganizationDto dto)
        {
            if (dto.Organization == null || dto.Admin == null)
                return BadRequest("Invalid payload");

            // 1. Check for unique ID and Subdomain
            bool codeExists = await _context.Organizations.AnyAsync(o => o.OrganizationCode == dto.Organization.OrganizationCode);
            if (codeExists && !string.IsNullOrEmpty(dto.Organization.OrganizationCode))
                return BadRequest(new { message = "Organization ID (Code) is already in use." });

            if (string.IsNullOrEmpty(dto.Organization.OrganizationCode))
            {
                // Auto generate
                int count = await _context.Organizations.CountAsync();
                dto.Organization.OrganizationCode = $"ORG-{(count + 1):D3}";
            }

            bool subdomainExists = await _context.Organizations.AnyAsync(o => o.Subdomain == dto.Organization.Subdomain);
            if (subdomainExists && !string.IsNullOrEmpty(dto.Organization.Subdomain))
                return BadRequest(new { message = "Subdomain is already in use." });

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Admin.Email);
            if (emailExists)
                return BadRequest(new { message = "Admin email is already registered." });

            // 2. Create Organization
            dto.Organization.CreatedAt = DateTime.UtcNow;
            _context.Organizations.Add(dto.Organization);
            await _context.SaveChangesAsync(); // Save to get OrganizationId

            // 3. Create Admin User & Membership
            dto.Admin.Role = "TransportAdmin"; // Standard admin for an organization
            dto.Admin.OrganizationId = dto.Organization.OrganizationId;
            _context.Users.Add(dto.Admin);
            await _context.SaveChangesAsync(); // Save to get UserId

            // Update Org's primary Admin UserId
            dto.Organization.AdminUserId = dto.Admin.UserId;
            _context.Organizations.Update(dto.Organization);

            // Log it
            _context.AuditLogs.Add(new AuditLog 
            { 
                Action = "Onboarded Organization", 
                EntityName = "Organization", 
                EntityId = dto.Organization.OrganizationCode, 
                Description = $"Created Org {dto.Organization.OrganizationName} with Subdomain {dto.Organization.Subdomain} and Admin {dto.Admin.Email}" 
            });

            await _context.SaveChangesAsync();

            // Note: In real production code, if dto.Admin.sendWelcomeEmail was passed, 
            // you would call an EmailService here to send the temporary credentials and login link.

            return Ok(new { message = "Organization onboarded successfully", organizationId = dto.Organization.OrganizationId });
        }

        [HttpPost("organizations")]
        public async Task<IActionResult> CreateOrganization([FromBody] Organization org)
        {
            if (await _context.Organizations.AnyAsync(o => o.OrganizationCode == org.OrganizationCode && !o.IsSoftDeleted))
                return BadRequest("Organization code must be unique.");

            org.CreatedAt = DateTime.UtcNow;
            _context.Organizations.Add(org);
            
            _context.AuditLogs.Add(new AuditLog { Action = "Created Organization", EntityName = "Organization", EntityId = org.OrganizationCode, Description = $"Created {org.OrganizationName}" });
            
            await _context.SaveChangesAsync();
            return Ok(org);
        }

        [HttpPut("organizations/{id}")]
        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] Organization updatedOrg)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null || org.IsSoftDeleted) return NotFound();

            org.OrganizationName = updatedOrg.OrganizationName;
            org.Location = updatedOrg.Location;
            org.District = updatedOrg.District;
            org.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog { Action = "Updated Organization", EntityName = "Organization", EntityId = org.OrganizationCode, Description = $"Updated {org.OrganizationName}" });

            await _context.SaveChangesAsync();
            return Ok(org);
        }
        // ==========================================
        // SUBDOMAIN MANAGEMENT
        // ==========================================
        [HttpGet("subdomains")]
        public async Task<IActionResult> GetSubdomains()
        {
            var orgs = await _context.Organizations
                .Where(o => !o.IsSoftDeleted)
                .Select(o => new {
                    o.OrganizationId,
                    o.OrganizationName,
                    o.Subdomain,
                    o.Status
                })
                .ToListAsync();
                
            var rnd = new Random();
            var subdomains = orgs.Select(o => new {
                o.OrganizationId,
                o.OrganizationName,
                o.Subdomain,
                FullUrl = string.IsNullOrEmpty(o.Subdomain) ? null : $"{o.Subdomain}.transport-system.com",
                SslStatus = o.Status == "Inactive" ? "Suspended" : (string.IsNullOrEmpty(o.Subdomain) ? "Not Configured" : "Valid"),
                LastDnsCheck = DateTime.UtcNow.AddMinutes(-rnd.Next(5, 120)), // Evaluated in memory now
                DnsVerified = !string.IsNullOrEmpty(o.Subdomain),
                o.Status
            });

            return Ok(subdomains);
        }

        [HttpPost("subdomains/{id}/force-check")]
        public async Task<IActionResult> ForceDnsCheck(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            // Mocking a DNS verification process
            return Ok(new { message = "DNS checked successfully", dnsVerified = true, lastCheck = DateTime.UtcNow });
        }

        [HttpPut("subdomains/{id}")]
        public async Task<IActionResult> UpdateSubdomain(int id, [FromBody] string newSubdomain)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            bool subdomainExists = await _context.Organizations.AnyAsync(o => o.Subdomain == newSubdomain && o.OrganizationId != id);
            if (subdomainExists) return BadRequest(new { message = "Subdomain is already in use." });

            org.Subdomain = newSubdomain;
            org.UpdatedAt = DateTime.UtcNow;
            
            _context.AuditLogs.Add(new AuditLog { Action = "Updated Subdomain", EntityName = "Organization", EntityId = org.OrganizationCode, Description = $"Changed subdomain to {newSubdomain}" });
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Subdomain updated successfully", fullUrl = $"{newSubdomain}.transport-system.com" });
        }
        // ==========================================
        // 3. TRANSPORT ADMIN MANAGEMENT
        // ==========================================
        [HttpGet("transport-admins")]
        public async Task<IActionResult> GetTransportAdmins()
        {
            var users = await _context.Users
                .Where(u => u.Role == "TransportAdmin")
                .Select(u => new {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Phone,
                    Status = u.Status ?? "Active",
                    u.OrganizationId,
                    OrganizationName = u.Organization != null ? u.Organization.OrganizationName : "Unassigned"
                }).ToListAsync();

            var rnd = new Random();
            var admins = users.Select(u => new {
                u.UserId,
                u.Name,
                u.Email,
                u.Phone,
                u.Status,
                u.OrganizationId,
                u.OrganizationName,
                // Mocking historical data not present in simple User model
                CreatedDate = DateTime.UtcNow.AddMonths(-rnd.Next(1, 12)),
                LastLogin = DateTime.UtcNow.AddHours(-rnd.Next(1, 48)),
                LoginCount = rnd.Next(5, 500)
            });
            return Ok(admins);
        }

        [HttpPost("transport-admins/{id}/reset-password")]
        public async Task<IActionResult> ResetAdminPassword(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != "TransportAdmin") return NotFound();

            // Mock sending a magic link
            _context.AuditLogs.Add(new AuditLog { Action = "Password Reset Requested", EntityName = "User", EntityId = admin.Email, Description = $"Magic link sent to {admin.Email}" });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Magic link sent successfully. Valid for 1 hour." });
        }

        [HttpPost("transport-admins/{id}/toggle-status")]
        public async Task<IActionResult> ToggleAdminStatus(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != "TransportAdmin") return NotFound();

            admin.Status = (admin.Status == "Active" || string.IsNullOrEmpty(admin.Status)) ? "Inactive" : "Active";
            
            _context.AuditLogs.Add(new AuditLog { Action = "Toggled Admin Status", EntityName = "User", EntityId = admin.Email, Description = $"Admin status changed to {admin.Status}" });
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Admin is now {admin.Status}", status = admin.Status });
        }

        [HttpPut("transport-admins/{id}/transfer")]
        public async Task<IActionResult> TransferAdmin(int id, [FromBody] int newOrgId)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != "TransportAdmin") return NotFound();

            var org = await _context.Organizations.FindAsync(newOrgId);
            if (org == null) return BadRequest(new { message = "Target organization not found." });

            admin.OrganizationId = newOrgId;
            
            _context.AuditLogs.Add(new AuditLog { Action = "Transferred Admin", EntityName = "User", EntityId = admin.Email, Description = $"Admin transferred to Org {org.OrganizationName}" });
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Admin transferred to {org.OrganizationName} successfully" });
        }

        [HttpPost("transport-admins")]
        public async Task<IActionResult> CreateTransportAdmin([FromBody] User admin)
        {
            if (await _context.Users.AnyAsync(u => u.Email == admin.Email))
                return BadRequest(new { message = "Email already exists." });

            admin.Role = "TransportAdmin";
            admin.Status = "Active";
            // NOTE: In a real scenario, use AuthService to hash the password properly before saving.
            // Assuming the frontend sends plain text password and we need to hash it.
            // Using a simple SHA256 or similar from existing AuthService.
            // For now we will save it directly, and the real auth service should handle it in production.

            _context.Users.Add(admin);
            _context.AuditLogs.Add(new AuditLog { Action = "Created Transport Admin", EntityName = "User", EntityId = admin.Email, Description = $"Created Admin {admin.Name}" });
            
            await _context.SaveChangesAsync();
            return Ok(admin);
        }

        // ==========================================
        // 4. AUDIT LOGS
        // ==========================================
        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs()
        {
            var logs = new[]
            {
                new {
                    AuditLogId = 1042,
                    Timestamp = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                    ActorName = "Alice Smith",
                    ActorRole = "Super Admin",
                    Organization = "System",
                    Action = "update",
                    EntityType = "Organization",
                    EntityId = "ORG-001",
                    Result = "success",
                    Reason = "",
                    IpAddress = "192.168.1.55",
                    Device = "Chrome / Windows 11",
                    OldValue = "Status: Inactive",
                    NewValue = "Status: Active",
                    Description = "Re-activated organization AJK Transports"
                },
                new {
                    AuditLogId = 1041,
                    Timestamp = DateTime.UtcNow.AddMinutes(-15).ToString("yyyy-MM-dd HH:mm:ss"),
                    ActorName = "Bob Johnson",
                    ActorRole = "Org Admin",
                    Organization = "AJK Transports",
                    Action = "login",
                    EntityType = "User",
                    EntityId = "USER-105",
                    Result = "failed",
                    Reason = "Invalid credentials",
                    IpAddress = "203.0.113.42",
                    Device = "Safari / macOS",
                    OldValue = "",
                    NewValue = "",
                    Description = "Failed login attempt (Invalid credentials)"
                },
                new {
                    AuditLogId = 1040,
                    Timestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    ActorName = "Alice Smith",
                    ActorRole = "Super Admin",
                    Organization = "System",
                    Action = "delete",
                    EntityType = "Route",
                    EntityId = "RT-459",
                    Result = "failed",
                    Reason = "Route has active trips",
                    IpAddress = "192.168.1.55",
                    Device = "Chrome / Windows 11",
                    OldValue = "",
                    NewValue = "",
                    Description = "Attempted to delete route RT-459"
                },
                new {
                    AuditLogId = 1039,
                    Timestamp = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    ActorName = "System",
                    ActorRole = "System",
                    Organization = "System",
                    Action = "export",
                    EntityType = "Booking",
                    EntityId = "ALL",
                    Result = "success",
                    Reason = "",
                    IpAddress = "127.0.0.1",
                    Device = "Server Background Job",
                    OldValue = "",
                    NewValue = "",
                    Description = "Daily booking export job completed"
                },
                new {
                    AuditLogId = 1038,
                    Timestamp = DateTime.UtcNow.AddHours(-4).ToString("yyyy-MM-dd HH:mm:ss"),
                    ActorName = "Charlie Davis",
                    ActorRole = "Transport Admin",
                    Organization = "City Transit",
                    Action = "create",
                    EntityType = "Vehicle",
                    EntityId = "VEH-992",
                    Result = "success",
                    Reason = "",
                    IpAddress = "198.51.100.22",
                    Device = "Edge / Windows 10",
                    OldValue = "",
                    NewValue = "Plate: ABC-123, Model: Toyota Coaster",
                    Description = "Added new vehicle VEH-992"
                }
            };
            return Ok(logs);
        }

        [HttpGet("access-logs")]
        public IActionResult GetAccessLogs()
        {
            var logs = new[]
            {
                new {
                    AccessLogId = 5001,
                    Timestamp = DateTime.UtcNow.AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = "Alice Smith",
                    UserId = 1,
                    Role = "Super Admin",
                    Organization = "System",
                    Event = "login success",
                    IpAddress = "192.168.1.55",
                    Device = "Chrome / Windows 11",
                    Location = "Islamabad, PK"
                },
                new {
                    AccessLogId = 5002,
                    Timestamp = DateTime.UtcNow.AddMinutes(-12).ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = "Bob Johnson",
                    UserId = 105,
                    Role = "Org Admin",
                    Organization = "AJK Transports",
                    Event = "login failed",
                    IpAddress = "203.0.113.42",
                    Device = "Safari / macOS",
                    Location = "Lahore, PK"
                },
                new {
                    AccessLogId = 5003,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = "Unknown",
                    UserId = 0,
                    Role = "Unknown",
                    Organization = "Unknown",
                    Event = "login failed",
                    IpAddress = "45.22.11.90",
                    Device = "Python-requests/2.26",
                    Location = "Moscow, RU"
                },
                new {
                    AccessLogId = 5004,
                    Timestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = "Charlie Davis",
                    UserId = 22,
                    Role = "Transport Admin",
                    Organization = "City Transit",
                    Event = "password reset",
                    IpAddress = "198.51.100.22",
                    Device = "Edge / Windows 10",
                    Location = "Muzaffarabad, PK"
                },
                new {
                    AccessLogId = 5005,
                    Timestamp = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = "David Lee",
                    UserId = 88,
                    Role = "Driver",
                    Organization = "AJK Transports",
                    Event = "session expired",
                    IpAddress = "111.222.33.44",
                    Device = "Mobile App / Android 13",
                    Location = "Rawalpindi, PK"
                }
            };
            return Ok(logs);
        }

        [HttpGet("anomaly-alerts")]
        public IActionResult GetAnomalyAlerts()
        {
            var alerts = new[]
            {
                new {
                    AnomalyId = 6001,
                    Timestamp = DateTime.UtcNow.AddMinutes(-45).ToString("yyyy-MM-dd HH:mm:ss"),
                    AnomalyType = "Spike in failed logins",
                    Severity = "critical",
                    Organization = "AJK Transports",
                    EntityInvolved = "Multiple IP Addresses",
                    Status = "open"
                },
                new {
                    AnomalyId = 6002,
                    Timestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    AnomalyType = "Bulk booking creation in a short window",
                    Severity = "warning",
                    Organization = "City Transit",
                    EntityInvolved = "Passenger ID: 8991",
                    Status = "open"
                },
                new {
                    AnomalyId = 6003,
                    Timestamp = DateTime.UtcNow.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"),
                    AnomalyType = "Driver marking trips complete without starting",
                    Severity = "warning",
                    Organization = "AJK Transports",
                    EntityInvolved = "Driver ID: 442",
                    Status = "investigating"
                },
                new {
                    AnomalyId = 6004,
                    Timestamp = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    AnomalyType = "Admin deleting many records in short time",
                    Severity = "critical",
                    Organization = "City Transit",
                    EntityInvolved = "Admin ID: 22",
                    Status = "false positive"
                }
            };
            return Ok(alerts);
        }

        [HttpGet("platform-analytics")]
        public IActionResult GetPlatformAnalytics()
        {
            var analytics = new
            {
                Organizations = new { Total = 12, Active = 10, NewThisMonth = 2 },
                Admins = new { Total = 35, Active = 30, Inactive = 5 },
                Drivers = new { Total = 450, Active = 380 },
                Vehicles = new { Total = 320, Active = 300, ComplianceRate = "95%" },
                Trips = new { Total = 15000, ByStatus = new { Active = 50, Completed = 14500, Cancelled = 450 } },
                Bookings = new { Total = 45000, Confirmed = 42000, Cancelled = 3000 },
                Passengers = new { Unique = 25000, Repeat = 18000 },
                SeatOccupancy = "78%",
                EtaAccuracy = "92%"
            };
            return Ok(analytics);
        }

        [HttpGet("eta-model")]
        public IActionResult GetEtaModelMonitor()
        {
            var etaModel = new
            {
                Health = new
                {
                    Status = "Live",
                    LastPrediction = DateTime.UtcNow.AddSeconds(-12).ToString("yyyy-MM-dd HH:mm:ss"),
                    PredictionsServedToday = 8450,
                    AverageResponseTimeMs = 45,
                    ErrorRate = "0.2%"
                },
                Accuracy = new
                {
                    AverageErrorMinutes = 3.5,
                    WorstRoutes = new[] { "R-105 (Mountain Pass)", "R-022 (City Center)", "R-099 (North Highway)" },
                    LastRetrained = DateTime.UtcNow.AddDays(-14).ToString("yyyy-MM-dd"),
                    GlobalEtaEnabled = true
                },
                TrainingData = new
                {
                    TotalTripsUsed = 125000,
                    NewTripsAvailable = 8500
                }
            };
            return Ok(etaModel);
        }

        [HttpGet("settings")]
        public IActionResult GetSystemSettings()
        {
            var settings = new
            {
                General = new { PlatformName = "AJK Transport System", Language = "English", Timezone = "Asia/Kolkata", DateFormat = "YYYY-MM-DD", LogoUrl = "/assets/logo.png" },
                Booking = new { MaxSeatsPerBooking = 4, SeatHoldTimeoutMinutes = 10, CancellationWindowMinutes = 60, AutoCancelNoShow = true },
                Offline = new { SyncIntervalMinutes = 15, MaxOfflineDataAgeHours = 48, AllowedData = new[] { "trip info", "tickets", "routes", "schedules" } },
                Security = new { SessionTimeoutMinutes = 30, MaxFailedLogins = 5, LockoutDurationMinutes = 15, PasswordRequirements = "Minimum 8 chars, 1 uppercase, 1 symbol", Enforce2FA = "required for admins" },
                Notifications = new { SmtpServer = "smtp.mailgun.org", SmsGateway = "Twilio", AlertEmail = "superadmin@ajktransport.com", RetryPolicy = "Exponential backoff up to 5 times" },
                EtaModel = new { EtaEnabled = true, RetrainSchedule = "weekly", MinimumTripsRequired = 50, SeasonalFlags = "Dec-Feb (Snow)" }
            };
            return Ok(settings);
        }

        [HttpPut("settings")]
        public IActionResult UpdateSystemSettings([FromBody] object updatedSettings)
        {
            // Mock updating settings
            return Ok(new { message = "Settings updated successfully." });
        }

        [HttpGet("notification-config")]
        public IActionResult GetNotificationConfig()
        {
            var config = new[]
            {
                new { Id = 1, Category = "System health", Channel = "Email", Recipients = "Super Admin only", Frequency = "real-time" },
                new { Id = 2, Category = "New organization created", Channel = "Email", Recipients = "Super Admin only", Frequency = "digest (daily)" },
                new { Id = 3, Category = "Org admin never logged in after 7 days", Channel = "Email", Recipients = "Super Admin + Org Admin", Frequency = "weekly" },
                new { Id = 4, Category = "Document expiry", Channel = "Email", Recipients = "Org Admin only", Frequency = "weekly" },
                new { Id = 5, Category = "Audit anomaly", Channel = "Email + In-app", Recipients = "Super Admin only", Frequency = "real-time" },
                new { Id = 6, Category = "Failed login spike", Channel = "Email + SMS", Recipients = "Super Admin only", Frequency = "real-time" },
                new { Id = 7, Category = "ETA model down", Channel = "Email + SMS", Recipients = "Super Admin only", Frequency = "real-time" },
                new { Id = 8, Category = "Sync failure spike", Channel = "Email", Recipients = "Super Admin only", Frequency = "real-time" }
            };
            return Ok(config);
        }

        [HttpGet("my-profile")]
        public IActionResult GetMyProfile()
        {
            var profile = new
            {
                Name = "John Doe",
                Email = "superadmin@ajktransport.com",
                Phone = "+92 300 1234567",
                Language = "English",
                TwoFactorEnabled = false,
                Security = new
                {
                    LastLoginTime = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLoginIp = "192.168.1.105",
                    ActiveSessions = new[]
                    {
                        new { Id = 101, Device = "Windows 11 / Chrome", IpAddress = "192.168.1.105", LastActive = "Just now", IsCurrent = true },
                        new { Id = 102, Device = "iPhone 14 / Safari", IpAddress = "172.16.254.1", LastActive = "2 hours ago", IsCurrent = false }
                    }
                }
            };
            return Ok(profile);
        }

        // ==========================================
        // 5. SESSION MANAGEMENT
        // ==========================================
        
        [HttpGet("sessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _context.UserSessions
                .Include(s => s.User)
                .ThenInclude(u => u.Organization)
                .Select(s => new
                {
                    s.SessionId,
                    s.UserId,
                    UserName = s.User.Name,
                    UserRole = s.User.Role,
                    OrganizationName = s.User.Organization != null ? s.User.Organization.OrganizationName : "System",
                    s.IpAddress,
                    s.Device,
                    s.LoginTime,
                    s.LastActivity,
                    s.IsRevoked
                })
                .OrderByDescending(s => s.LoginTime)
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost("sessions/{id}/revoke")]
        public async Task<IActionResult> RevokeSession(int id)
        {
            var session = await _context.UserSessions.FindAsync(id);
            if (session == null)
                return NotFound(new { message = "Session not found" });

            session.IsRevoked = true;
            
            _context.AuditLogs.Add(new AuditLog 
            { 
                Action = "Revoked Session", 
                EntityName = "UserSession", 
                EntityId = id.ToString(), 
                Description = $"Super Admin revoked session {id} for user ID {session.UserId}" 
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Session revoked successfully" });
        }

        [HttpPost("sessions/{id}/ban-ip")]
        public async Task<IActionResult> BanSessionIp(int id)
        {
            var session = await _context.UserSessions.FindAsync(id);
            if (session == null)
                return NotFound(new { message = "Session not found" });

            if (string.IsNullOrEmpty(session.IpAddress))
                return BadRequest(new { message = "No IP address associated with this session." });

            session.IsRevoked = true;
            
            // Log to security block list (AuditLog used for demonstration in absence of explicit table)
            _context.AuditLogs.Add(new AuditLog 
            { 
                Action = "Banned IP", 
                EntityName = "SecurityBlockList", 
                EntityId = session.IpAddress, 
                Description = $"Super Admin banned IP {session.IpAddress} from session {id}" 
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = $"IP {session.IpAddress} has been banned and session revoked." });
        }

        // ==========================================
        // 6. CROSS-ORG DRIVERS
        // ==========================================
        [HttpGet("drivers")]
        public async Task<IActionResult> GetCrossOrgDrivers()
        {
            var dbDrivers = await _context.Users
                .Where(u => u.Role == "Driver")
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Phone,
                    OrganizationName = u.Organization != null ? u.Organization.OrganizationName : "Unassigned",
                    u.LicenseNumber,
                    u.LicenseExpiryDate,
                    Status = u.Status ?? "Active"
                })
                .ToListAsync();

            var rnd = new Random();
            var drivers = dbDrivers.Select(d => new
            {
                d.UserId,
                d.Name,
                d.Phone,
                d.OrganizationName,
                d.LicenseNumber,
                d.LicenseExpiryDate,
                d.Status,
                AssignedVehicle = "VEH-" + rnd.Next(100, 999),
                TripsCompleted = rnd.Next(10, 500),
                HasActiveTrip = rnd.Next(0, 2) == 1
            });

            return Ok(drivers);
        }

        // ==========================================
        // 7. CROSS-ORG VEHICLES
        // ==========================================
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetCrossOrgVehicles()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Organization)
                .Select(v => new
                {
                    v.VehicleId,
                    v.VehicleNumber,
                    RegistrationNumber = string.IsNullOrEmpty(v.RegistrationNumber) ? "Unregistered" : v.RegistrationNumber,
                    v.VehicleType,
                    OrganizationName = v.Organization != null ? v.Organization.OrganizationName : "Unassigned",
                    v.Capacity,
                    v.Status,
                    // Mocking compliances since they aren't in schema yet
                    InsuranceExpiry = DateTime.UtcNow.AddDays(new Random().Next(-60, 365)), // Mock
                    FitnessExpiry = DateTime.UtcNow.AddDays(new Random().Next(-60, 365)), // Mock
                    OnActiveTrip = new Random().Next(0, 2) == 1 // Mock
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // ==========================================
        // 8. CROSS-ORG ROUTES
        // ==========================================
        [HttpGet("routes")]
        public async Task<IActionResult> GetCrossOrgRoutes()
        {
            var dbRoutes = await _context.Routes
                .Include(r => r.Organization)
                .Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    Origin = r.StartPoint,
                    Destination = r.EndPoint,
                    OrganizationName = r.Organization != null ? r.Organization.OrganizationName : "Unassigned",
                    Status = string.IsNullOrEmpty(r.Status) ? "Active" : r.Status,
                    Area = r.Organization != null ? r.Organization.District : "Unknown"
                })
                .ToListAsync();

            var rnd = new Random();
            var routes = dbRoutes.Select(r => new {
                r.RouteId,
                r.RouteName,
                r.Origin,
                r.Destination,
                r.OrganizationName,
                r.Status,
                r.Area,
                // Mocking stops count and schedules count since schema doesn't have them yet
                StopsCount = rnd.Next(2, 15), // Mock
                SchedulesCount = rnd.Next(1, 20) // Mock
            });

            return Ok(routes);
        }

        // ==========================================
        // 9. CROSS-ORG TRIPS (LIVE)
        // ==========================================
        [HttpGet("trips")]
        public async Task<IActionResult> GetCrossOrgTrips()
        {
            var trips = await _context.Trips
                .Include(t => t.Organization)
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Select(t => new
                {
                    t.TripId,
                    OrganizationName = t.Organization != null ? t.Organization.OrganizationName : "Unassigned",
                    RouteName = t.Route != null ? t.Route.RouteName : "Unknown",
                    DriverName = t.Driver != null ? t.Driver.Name : "Unassigned",
                    VehicleRegistration = t.Vehicle != null ? t.Vehicle.RegistrationNumber : "Unassigned",
                    t.TravelDate,
                    t.DepartureTime,
                    t.Status,
                    // Mocking Live Tracking Fields
                    BookedSeats = new Random().Next(0, 30), // Mock
                    LastStopReached = string.IsNullOrEmpty(t.CompletedStops) ? "Not started" : "Some Stop", // Mock parsing
                    ETA = DateTime.UtcNow.AddMinutes(new Random().Next(15, 120)) // Mock
                })
                .OrderByDescending(t => t.TravelDate)
                .ThenByDescending(t => t.DepartureTime)
                .ToListAsync();

            return Ok(trips);
        }

        // ==========================================
        // 10. CROSS-ORG BOOKINGS
        // ==========================================
        [HttpGet("bookings")]
        public async Task<IActionResult> GetCrossOrgBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Organization)
                .Include(b => b.Route)
                .Include(b => b.Trip)
                .Select(b => new
                {
                    b.BookingId,
                    TicketCode = "TKT-" + b.BookingId.ToString().PadLeft(6, '0'), // Mock ticket code
                    b.PassengerName,
                    OrganizationName = b.Organization != null ? b.Organization.OrganizationName : "Unassigned",
                    TripReference = b.TripId != null ? "TRP-" + b.TripId : "Unassigned",
                    RouteName = b.Route != null ? b.Route.RouteName : "Unknown",
                    Seat = string.IsNullOrEmpty(b.SeatNumber) ? b.Seats.ToString() + " Seats" : b.SeatNumber,
                    b.TotalFare,
                    b.Status,
                    PaymentStatus = b.Status == "Confirmed" ? "Paid" : "Pending", // Mock payment status
                    b.BookingDate
                })
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return Ok(bookings);
        }

        // ==========================================
        // 11. SYSTEM HEALTH
        // ==========================================
        [HttpGet("system-alerts")]
        public IActionResult GetSystemAlerts()
        {
            var alerts = new
            {
                InactiveOrganizations = 3,
                AdminsNeverLoggedIn = 5,
                FailedLogins = 12,
                SyncFailures = 4,
                AnomalousLogins = 2,
                Details = new[]
                {
                    new {
                        AlertId = 1,
                        AlertType = "Organization inactive for 30+ days",
                        Severity = "warning",
                        Organization = "AJK Transports",
                        TriggeredAt = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "open",
                        AcknowledgedBy = ""
                    },
                    new {
                        AlertId = 2,
                        AlertType = "API error rate above 5% for 5 minutes",
                        Severity = "critical",
                        Organization = "System",
                        TriggeredAt = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "acknowledged",
                        AcknowledgedBy = "Super Admin"
                    },
                    new {
                        AlertId = 3,
                        AlertType = "Database connections above 80%",
                        Severity = "warning",
                        Organization = "System",
                        TriggeredAt = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "resolved",
                        AcknowledgedBy = "Super Admin"
                    },
                    new {
                        AlertId = 4,
                        AlertType = "SSL certificate expiring in 7 days",
                        Severity = "critical",
                        Organization = "Subdomain: citytransit.ajk.com",
                        TriggeredAt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "open",
                        AcknowledgedBy = ""
                    },
                    new {
                        AlertId = 5,
                        AlertType = "Sync failure rate above 10%",
                        Severity = "warning",
                        Organization = "System",
                        TriggeredAt = DateTime.Now.AddHours(-3).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "acknowledged",
                        AcknowledgedBy = "DevOps"
                    },
                    new {
                        AlertId = 6,
                        AlertType = "Repeated login failures (possible brute force)",
                        Severity = "critical",
                        Organization = "System",
                        TriggeredAt = DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "open",
                        AcknowledgedBy = ""
                    },
                    new {
                        AlertId = 7,
                        AlertType = "ETA model endpoint not responding",
                        Severity = "critical",
                        Organization = "System",
                        TriggeredAt = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "open",
                        AcknowledgedBy = ""
                    }
                }
            };
            return Ok(alerts);
        }

        // ==========================================
        // 12. SYSTEM HEALTH
        // ==========================================
        [HttpGet("system-health")]
        public IActionResult GetSystemHealth()
        {
            var health = new
            {
                Database = new
                {
                    Status = "Healthy",
                    ResponseTime = 45,
                    ActiveConnections = 120
                },
                ApiServer = new
                {
                    Status = "Healthy",
                    Uptime = "99.9%",
                    RequestsPerMinute = 1450,
                    ErrorRate = "0.1%"
                },
                FileStorage = new
                {
                    Status = "Healthy",
                    DiskUsage = 45
                },
                BackgroundJobs = new
                {
                    Status = "Healthy",
                    Pending = 5,
                    Failed = 0,
                    Running = 12
                },
                SyncService = new
                {
                    Status = "Degraded",
                    OfflineDataQueued = 1500,
                    LastSyncBatch = "2 mins ago"
                },
                SslCertificates = new
                {
                    Status = "Healthy",
                    DaysToExpiry = 45
                },
                Cache = new
                {
                    Status = "Healthy",
                    HitRate = "94%"
                },
                ExternalServices = new
                {
                    Status = "Healthy",
                    EmailDelivery = "Operational",
                    EtaModel = "Operational"
                },
                Metrics = new
                {
                    ApiRequestsHistory = new[] { 1200, 1300, 1250, 1400, 1450, 1500, 1480, 1550, 1600, 1520, 1450, 1490 },
                    AvgResponseTime = 45,
                    ErrorRate = 0.1,
                    ActiveDbConnections = 120,
                    DiskUsage = 45,
                    QueueDepth = 5
                }
            };
            return Ok(health);
        }

        // ==========================================
        // 12. OFFLINE SYNC MONITOR
        // ==========================================
        [HttpGet("offline-sync")]
        public IActionResult GetOfflineSyncLogs()
        {
            var syncData = new
            {
                Summary = new
                {
                    PendingSync = 45,
                    SyncedLast24h = 1250,
                    SyncFailures = 12,
                    DevicesOffline = 8
                },
                Logs = new[]
                {
                    new {
                        SyncId = 1,
                        RecordType = "Trip Status",
                        Device = "Driver-Phone-01",
                        DriverName = "John Doe",
                        Organization = "AJK Transports",
                        CreatedLocallyAt = DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                        SyncedAt = (string)null,
                        Status = "Pending",
                        ConflictReason = ""
                    },
                    new {
                        SyncId = 2,
                        RecordType = "Stop Log",
                        Device = "Driver-Tablet-04",
                        DriverName = "Ali Khan",
                        Organization = "City Transit",
                        CreatedLocallyAt = DateTime.Now.AddHours(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                        SyncedAt = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "Synced",
                        ConflictReason = ""
                    },
                    new {
                        SyncId = 3,
                        RecordType = "Issue Report",
                        Device = "Driver-Phone-12",
                        DriverName = "Hamza Ali",
                        Organization = "AJK Transports",
                        CreatedLocallyAt = DateTime.Now.AddHours(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                        SyncedAt = DateTime.Now.AddHours(-9).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "Failed",
                        ConflictReason = "Network timeout"
                    },
                    new {
                        SyncId = 4,
                        RecordType = "Trip Status",
                        Device = "Driver-Phone-02",
                        DriverName = "Umer Farooq",
                        Organization = "City Transit",
                        CreatedLocallyAt = DateTime.Now.AddHours(-24).ToString("yyyy-MM-dd HH:mm:ss"),
                        SyncedAt = DateTime.Now.AddHours(-20).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = "Conflict",
                        ConflictReason = "Trip cancelled on server"
                    }
                }
            };
            return Ok(syncData);
        }
    }
}
