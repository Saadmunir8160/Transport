using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _context.Routes.ToListAsync();
            return Ok(routes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();
            return Ok(route);
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetRouteDetails(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            var timings = await _context.Timings
                .Where(t => t.RouteId == id)
                .ToListAsync();

            var fares = await _context.Fares
                .Where(f => f.RouteId == id)
                .ToListAsync();

            var timingsWithVehicles = new List<object>();
            foreach (var timing in timings)
            {
                var vehicle = await _context.Vehicles.FindAsync(timing.VehicleId);
                timingsWithVehicles.Add(new
                {
                    timingId = timing.TimingId,
                    vehicleId = timing.VehicleId,
                    vehicleNumber = vehicle?.VehicleNumber,
                    vehicleType = vehicle?.VehicleType,
                    capacity = vehicle?.Capacity,
                    departureTime = timing.DepartureTime.ToString(@"hh\:mm"),
                    arrivalTime = timing.ArrivalTime.ToString(@"hh\:mm")
                });
            }

            return Ok(new
            {
                route = route,
                timings = timingsWithVehicles,
                fares = fares
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] RouteDto dto)
        {
            var route = new Models.Route
            {
                RouteName = dto.RouteName,
                StartPoint = dto.StartPoint,
                EndPoint = dto.EndPoint,
                Distance = dto.Distance
            };
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Auto-generate fares based on distance
            var baseFare = 100;
            var perKmRate = 12;
            var adultFare = baseFare + (route.Distance * perKmRate);

            var fares = new List<Fare>
            {
                new Fare { RouteId = route.RouteId, PassengerType = "Adult", Amount = adultFare },
                new Fare { RouteId = route.RouteId, PassengerType = "Child", Amount = (int)(adultFare * 0.5m) },
                new Fare { RouteId = route.RouteId, PassengerType = "Student", Amount = (int)(adultFare * 0.7m) },
                new Fare { RouteId = route.RouteId, PassengerType = "Senior", Amount = (int)(adultFare * 0.6m) }
            };

            _context.Fares.AddRange(fares);
            await _context.SaveChangesAsync();

            return Ok(new { route = route, fares = fares, message = "Route and fares created automatically!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteDto dto)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            route.RouteName = dto.RouteName;
            route.StartPoint = dto.StartPoint;
            route.EndPoint = dto.EndPoint;
            route.Distance = dto.Distance;

            await _context.SaveChangesAsync();
            return Ok(route);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Route deleted" });
        }
    }
}
