using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/transportadmin")]
    [ApiController]
    [Authorize(Roles = "TransportAdmin")]
    public class TransportAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransportAdminController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOrganizationId()
        {
            var orgIdClaim = User.Claims.FirstOrDefault(c => c.Type == "OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int orgId))
            {
                return orgId;
            }
            throw new Exception("Organization ID not found in token.");
        }

        // ==========================================
        // 1. DASHBOARD STATISTICS
        // ==========================================
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var orgId = GetCurrentOrganizationId();
                var today = DateTime.Today;

                var stats = new
                {
                    TodayTrips = await _context.Trips.CountAsync(t => t.OrganizationId == orgId && t.TravelDate.Date == today),
                    ActiveTrips = await _context.Trips.CountAsync(t => t.OrganizationId == orgId && t.Status == "Active"),
                    TodayBookings = await _context.Bookings.CountAsync(b => b.OrganizationId == orgId && b.BookingDate.Date == today),
                    SeatsAvailable = 45, // Dummy calculation for now
                    CancelledToday = await _context.Trips.CountAsync(t => t.OrganizationId == orgId && t.TravelDate.Date == today && t.Status == "Cancelled"),
                    ActiveDrivers = await _context.Users.CountAsync(u => u.Role == "Driver" && u.OrganizationId == orgId),
                    ActiveVehicles = await _context.Vehicles.CountAsync(v => v.OrganizationId == orgId && v.Status == "Active"),
                    
                    // Alerts Data
                    ExpiringLicences = 3, // Dummy
                    ExpiringVehicleDocs = 1, // Dummy
                    UnassignedTrips = await _context.Trips.CountAsync(t => t.OrganizationId == orgId && (t.DriverId == null || t.VehicleId == null) && t.Status == "Scheduled"),
                    OpenComplaints = await _context.Complaints.CountAsync(c => c.OrganizationId == orgId && c.Status == "Open"),
                    ActiveAdvisories = 1 // Dummy
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reports/bookings-timeline")]
        public async Task<IActionResult> GetBookingsTimeline()
        {
            try
            {
                var orgId = GetCurrentOrganizationId();
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);

                var bookings = await _context.Bookings
                    .Where(b => b.OrganizationId == orgId && b.BookingDate >= thirtyDaysAgo)
                    .GroupBy(b => b.BookingDate.Date)
                    .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), totalBookings = g.Count() })
                    .OrderBy(g => g.date)
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reports/trips-by-status")]
        public async Task<IActionResult> GetTripsByStatus()
        {
            try
            {
                var orgId = GetCurrentOrganizationId();
                
                var trips = await _context.Trips
                    .Where(t => t.OrganizationId == orgId)
                    .GroupBy(t => t.Status)
                    .Select(g => new { status = g.Key, count = g.Count() })
                    .ToListAsync();

                return Ok(trips);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reports/popular-routes")]
        public async Task<IActionResult> GetPopularRoutes()
        {
            try
            {
                var orgId = GetCurrentOrganizationId();

                var popularRoutes = await _context.Bookings
                    .Where(b => b.OrganizationId == orgId)
                    .GroupBy(b => b.RouteId)
                    .Select(g => new { 
                        RouteId = g.Key, 
                        TotalBookings = g.Count() 
                    })
                    .OrderByDescending(x => x.TotalBookings)
                    .Take(5)
                    .ToListAsync();

                // Fetch route names in memory to avoid complex join issues if routes aren't seeded correctly yet
                var routeIds = popularRoutes.Select(p => p.RouteId).ToList();
                var routes = await _context.Routes.Where(r => routeIds.Contains(r.RouteId)).ToDictionaryAsync(r => r.RouteId, r => r.RouteName);

                var result = popularRoutes.Select(p => new {
                    routeName = routes.ContainsKey(p.RouteId) ? routes[p.RouteId] : "Unknown Route",
                    totalBookings = p.TotalBookings
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // 2. DRIVER MANAGEMENT
        // ==========================================
        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var orgId = GetCurrentOrganizationId();
            var drivers = await _context.Users
                .Where(u => u.Role == "Driver" && u.OrganizationId == orgId)
                .Select(u => new { u.UserId, u.Name, u.Email, u.Phone, u.LicenseNumber, u.Status })
                .ToListAsync();
            return Ok(drivers);
        }

        [HttpPost("drivers")]
        public async Task<IActionResult> AddDriver([FromBody] User driver)
        {
            var orgId = GetCurrentOrganizationId();
            driver.Role = "Driver";
            driver.OrganizationId = orgId;
            driver.PasswordHash = Services.AuthService.HashPassword("Driver@123"); // Default password
            
            _context.Users.Add(driver);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Driver added successfully" });
        }

        // ==========================================
        // 3. VEHICLE MANAGEMENT
        // ==========================================
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            var orgId = GetCurrentOrganizationId();
            var vehicles = await _context.Vehicles
                .Where(v => v.OrganizationId == orgId)
                .ToListAsync();
            return Ok(vehicles);
        }

        [HttpPost("vehicles")]
        public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
        {
            var orgId = GetCurrentOrganizationId();
            vehicle.OrganizationId = orgId;
            
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Vehicle added successfully" });
        }

        // ==========================================
        // 4. ROUTE MANAGEMENT
        // ==========================================
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            var orgId = GetCurrentOrganizationId();
            var routes = await _context.Routes
                .Where(r => r.OrganizationId == orgId)
                .ToListAsync();
            return Ok(routes);
        }

        [HttpPost("routes")]
        public async Task<IActionResult> AddRoute([FromBody] Models.Route route)
        {
            var orgId = GetCurrentOrganizationId();
            route.OrganizationId = orgId;
            
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Route added successfully", routeId = route.RouteId });
        }

        [HttpPut("routes/{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] Models.Route routeUpdates)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();
            
            route.RouteName = routeUpdates.RouteName;
            route.StartPoint = routeUpdates.StartPoint;
            route.EndPoint = routeUpdates.EndPoint;
            route.Distance = routeUpdates.Distance;
            route.BaseFare = routeUpdates.BaseFare;
            route.BaselineDuration = routeUpdates.BaselineDuration;
            route.IsSeasonal = routeUpdates.IsSeasonal;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Route updated successfully" });
        }

        [HttpPut("routes/{id}/status")]
        public async Task<IActionResult> UpdateRouteStatus(int id, [FromBody] string status)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();
            
            // "A route with active schedules can be deactivated, not deleted." - the spec says deactivated.
            route.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Route status updated" });
        }

        [HttpGet("routes/{id}/stops")]
        public async Task<IActionResult> GetRouteStops(int id)
        {
            var stops = await _context.RouteStops
                .Where(s => s.RouteId == id)
                .OrderBy(s => s.StopOrder)
                .ToListAsync();
            return Ok(stops);
        }

        [HttpPost("routes/{id}/stops")]
        public async Task<IActionResult> AddRouteStop(int id, [FromBody] Models.RouteStop stop)
        {
            stop.RouteId = id;
            _context.RouteStops.Add(stop);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Stop added successfully", stopId = stop.RouteStopId });
        }

        [HttpPut("routes/{id}/stops/reorder")]
        public async Task<IActionResult> ReorderStops(int id, [FromBody] List<Models.RouteStop> stops)
        {
            foreach (var updatedStop in stops)
            {
                var stop = await _context.RouteStops.FirstOrDefaultAsync(s => s.RouteStopId == updatedStop.RouteStopId && s.RouteId == id);
                if (stop != null)
                {
                    stop.StopOrder = updatedStop.StopOrder;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Stops reordered successfully" });
        }

        // ==========================================
        // 5. SCHEDULE / TIMING MANAGEMENT
        // ==========================================
        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            var orgId = GetCurrentOrganizationId();
            var schedules = await _context.Timings
                .Include(t => t.Route)
                .Include(t => t.Vehicle)
                .Where(t => t.OrganizationId == orgId)
                .Select(t => new {
                    t.TimingId, t.RouteId, t.VehicleId,
                    RouteName = t.Route.RouteName,
                    VehicleNumber = t.Vehicle.VehicleNumber,
                    t.DepartureTime, t.ArrivalTime,
                    t.OperatingDays, t.Status
                })
                .ToListAsync();
            return Ok(schedules);
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> AddSchedule([FromBody] Timing timing)
        {
            var orgId = GetCurrentOrganizationId();
            timing.OrganizationId = orgId;
            
            _context.Timings.Add(timing);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Schedule added successfully" });
        }

        // ==========================================
        // 6. TRIP MANAGEMENT
        // ==========================================
        [HttpGet("trips")]
        public async Task<IActionResult> GetTrips()
        {
            var orgId = GetCurrentOrganizationId();
            var trips = await _context.Trips
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Where(t => t.OrganizationId == orgId)
                .Select(t => new {
                    t.TripId, t.TravelDate, t.DepartureTime, t.Status,
                    RouteName = t.Route.RouteName,
                    DriverName = t.Driver.Name,
                    VehicleNumber = t.Vehicle.VehicleNumber
                })
                .ToListAsync();
            return Ok(trips);
        }

        [HttpPost("trips")]
        public async Task<IActionResult> AddTrip([FromBody] Trip trip)
        {
            var orgId = GetCurrentOrganizationId();
            trip.OrganizationId = orgId;
            
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Trip added successfully" });
        }

        [HttpGet("trips/{id}/bookings")]
        public async Task<IActionResult> GetTripBookings(int id)
        {
            var orgId = GetCurrentOrganizationId();
            
            // Verify trip belongs to this org
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == id && t.OrganizationId == orgId);
            if (trip == null) return NotFound(new { message = "Trip not found." });

            var bookings = await _context.Bookings
                .Where(b => b.TripId == id)
                .Select(b => new {
                    b.BookingId,
                    b.PassengerName,
                    b.PassengerPhone,
                    b.PassengerCNIC,
                    b.Seats,
                    b.SeatNumber,
                    b.TotalFare,
                    b.Status,
                    b.BookingDate
                })
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return Ok(bookings);
        }
    }
}
