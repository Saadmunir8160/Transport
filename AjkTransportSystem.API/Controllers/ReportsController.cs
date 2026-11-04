using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Models.Trip> FilterTrips(ReportFilterDto filter)
        {
            var query = _context.Trips.Include(t => t.Route).Include(t => t.Vehicle).Include(t => t.Driver).AsQueryable();
            if (filter.StartDate.HasValue) query = query.Where(t => t.TravelDate >= filter.StartDate.Value);
            if (filter.EndDate.HasValue) query = query.Where(t => t.TravelDate <= filter.EndDate.Value);
            if (filter.RouteId.HasValue) query = query.Where(t => t.RouteId == filter.RouteId.Value);
            if (filter.DriverId.HasValue) query = query.Where(t => t.DriverId == filter.DriverId.Value);
            if (filter.VehicleId.HasValue) query = query.Where(t => t.VehicleId == filter.VehicleId.Value);
            return query;
        }

        [HttpPost("trips")]
        public async Task<IActionResult> GetTripsReport([FromBody] ReportFilterDto filter)
        {
            var trips = await FilterTrips(filter).ToListAsync();
            return Ok(trips);
        }

        [HttpPost("cancellations")]
        public async Task<IActionResult> GetCancellationsReport([FromBody] ReportFilterDto filter)
        {
            var trips = await FilterTrips(filter).Where(t => t.Status == "Cancelled").ToListAsync();
            return Ok(trips);
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> GetBookingsReport([FromBody] ReportFilterDto filter)
        {
            var query = _context.Bookings.Include(b => b.Trip).Include(b => b.Route).AsQueryable();
            if (filter.StartDate.HasValue) query = query.Where(b => b.TravelDate >= filter.StartDate.Value);
            if (filter.EndDate.HasValue) query = query.Where(b => b.TravelDate <= filter.EndDate.Value);
            if (filter.RouteId.HasValue) query = query.Where(b => b.RouteId == filter.RouteId.Value);
            if (filter.DriverId.HasValue) query = query.Where(b => b.Trip != null && b.Trip.DriverId == filter.DriverId.Value);
            if (filter.VehicleId.HasValue) query = query.Where(b => b.Trip != null && b.Trip.VehicleId == filter.VehicleId.Value);

            var bookings = await query.ToListAsync();
            return Ok(bookings);
        }

        [HttpPost("popular-routes")]
        public async Task<IActionResult> GetPopularRoutesReport([FromBody] ReportFilterDto filter)
        {
            var query = _context.Bookings.Include(b => b.Route).AsQueryable();
            if (filter.StartDate.HasValue) query = query.Where(b => b.TravelDate >= filter.StartDate.Value);
            if (filter.EndDate.HasValue) query = query.Where(b => b.TravelDate <= filter.EndDate.Value);
            
            var stats = await query
                .GroupBy(b => new { b.RouteId, b.Route.RouteName })
                .Select(g => new {
                    RouteId = g.Key.RouteId,
                    RouteName = g.Key.RouteName,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalFare)
                })
                .OrderByDescending(s => s.TotalBookings)
                .ToListAsync();

            return Ok(stats);
        }

        [HttpPost("vehicle-usage")]
        public async Task<IActionResult> GetVehicleUsageReport([FromBody] ReportFilterDto filter)
        {
            var query = FilterTrips(filter).Where(t => t.VehicleId.HasValue && t.Status == "Completed");
            var stats = await query
                .GroupBy(t => new { t.VehicleId, t.Vehicle.VehicleNumber })
                .Select(g => new {
                    VehicleId = g.Key.VehicleId,
                    VehicleNumber = g.Key.VehicleNumber,
                    TripsCompleted = g.Count(),
                    TotalDistance = g.Sum(t => t.Route.Distance)
                })
                .OrderByDescending(s => s.TripsCompleted)
                .ToListAsync();

            return Ok(stats);
        }
        
        [HttpPost("driver-activity")]
        public async Task<IActionResult> GetDriverActivityReport([FromBody] ReportFilterDto filter)
        {
            var query = FilterTrips(filter).Where(t => t.DriverId.HasValue && t.Status == "Completed");
            var stats = await query
                .GroupBy(t => new { t.DriverId, t.Driver.Name })
                .Select(g => new {
                    DriverId = g.Key.DriverId,
                    DriverName = g.Key.Name,
                    TripsCompleted = g.Count()
                })
                .OrderByDescending(s => s.TripsCompleted)
                .ToListAsync();

            return Ok(stats);
        }

        [HttpPost("export/pdf")]
        public IActionResult ExportToPdf([FromBody] ReportFilterDto filter)
        {
            return Ok(new { url = "https://example.com/reports/report.pdf" });
        }

        [HttpPost("export/excel")]
        public IActionResult ExportToExcel([FromBody] ReportFilterDto filter)
        {
            return Ok(new { url = "https://example.com/reports/report.xlsx" });
        }
    }
}
