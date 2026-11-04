using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using System.Security.Claims;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int orgId)) return orgId;
            return 1;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var orgId = GetCurrentOrganizationId();
            var today = DateTime.UtcNow.Date;
            var in30Days = today.AddDays(30);

            // KPI Cards
            var todayTrips = await _context.Trips.Where(t => t.OrganizationId == orgId && t.TravelDate.Date == today).ToListAsync();
            var activeTripsNow = todayTrips.Count(t => t.Status == "Active");
            var todaysBookings = await _context.Bookings.CountAsync(b => b.OrganizationId == orgId && b.BookingDate.Date == today);
            
            var seatsAvailable = todayTrips.Where(t => t.Status != "Cancelled").Sum(t => t.BookedSeats - t.BookedSeats);
            var cancelledToday = todayTrips.Count(t => t.Status == "Cancelled");

            var activeDrivers = await _context.Users.CountAsync(u => u.OrganizationId == orgId && u.Role == "Driver" && u.Status == "Active");
            var activeVehicles = await _context.Vehicles.CountAsync(v => v.OrganizationId == orgId && v.Status == "Active");

            // Charts Mock/Aggregate
            var tripsByStatus = await _context.Trips
                .Where(t => t.OrganizationId == orgId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Alerts
            var driversExpiring = await _context.Users.Where(u => u.OrganizationId == orgId && u.Role == "Driver" && u.LicenseExpiryDate.HasValue && u.LicenseExpiryDate <= in30Days).Select(u => new { u.Name, u.LicenseExpiryDate }).ToListAsync();
            var vehiclesExpiring = await _context.Vehicles.Where(v => v.OrganizationId == orgId && v.NextDocumentExpiry.HasValue && v.NextDocumentExpiry <= in30Days).Select(v => new { v.RegistrationNumber, v.NextDocumentExpiry }).ToListAsync();
            
            var tripsNoDriverOrVehicle = todayTrips.Where(t => (t.DriverId == null || t.VehicleId == null) && t.Status == "Scheduled").Select(t => new { t.TripId, t.DepartureTime }).ToList();
            
            var openComplaints = await _context.Complaints.CountAsync(c => c.OrganizationId == orgId && c.Status == "Open");
            var roadAdvisories = await _context.Advisories.Where(a => a.OrganizationId == orgId && a.Status == "Active").Select(a => new { a.AffectedRoutes, a.Reason }).ToListAsync();

            return Ok(new
            {
                KPIs = new {
                    TodayTrips = todayTrips.Count,
                    ActiveTripsNow = activeTripsNow,
                    TodaysBookings = todaysBookings,
                    SeatsAvailable = seatsAvailable,
                    CancelledToday = cancelledToday,
                    ActiveDrivers = activeDrivers,
                    ActiveVehicles = activeVehicles
                },
                Charts = new {
                    TripsByStatus = tripsByStatus
                },
                Alerts = new {
                    DriversExpiring = driversExpiring,
                    VehiclesExpiring = vehiclesExpiring,
                    TripsNoDriverOrVehicle = tripsNoDriverOrVehicle,
                    OpenComplaints = openComplaints,
                    RoadAdvisories = roadAdvisories
                }
            });
        }
    }
}
