using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;
using System.Linq;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesStopsExtensionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesStopsExtensionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("duplicate-return/{id}")]
        public async Task<IActionResult> DuplicateAsReturnRoute(int id)
        {
            var originalRoute = await _context.Routes.Include(r => r.Stops).FirstOrDefaultAsync(r => r.RouteId == id);
            if (originalRoute == null) return NotFound();

            var returnRoute = new Models.Route
            {
                RouteName = $"{originalRoute.RouteName} (Return)",
                StartPoint = originalRoute.EndPoint,
                EndPoint = originalRoute.StartPoint,
                Distance = originalRoute.Distance,
                Direction = originalRoute.Direction == "Outbound" ? "Inbound" : "Outbound",
                BaseFare = originalRoute.BaseFare,
                BaselineDuration = originalRoute.BaselineDuration,
                IsSeasonal = originalRoute.IsSeasonal,
                Status = "Active",
                OrganizationId = originalRoute.OrganizationId
            };
            
            _context.Routes.Add(returnRoute);
            await _context.SaveChangesAsync();
            
            var originalStops = originalRoute.Stops.OrderByDescending(s => s.StopOrder).ToList();
            int order = 1;
            foreach(var stop in originalStops)
            {
                _context.RouteStops.Add(new RouteStop
                {
                    RouteId = returnRoute.RouteId,
                    StopName = stop.StopName,
                    StopOrder = order++,
                    Latitude = stop.Latitude,
                    Longitude = stop.Longitude,
                    DistanceFromStart = (int)originalRoute.Distance - stop.DistanceFromStart,
                    FareFromStart = (originalRoute.BaseFare ?? 0m) - stop.FareFromStart, // simplified logic
                    MinutesFromDeparture = stop.MinutesFromDeparture
                });
            }
            await _context.SaveChangesAsync();

            return Ok(returnRoute);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateRouteStatus(int id, [FromBody] string status)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            route.Status = status;
            await _context.SaveChangesAsync();
            return Ok(route);
        }
        
        [HttpPost("{routeId}/stops/reorder")]
        public async Task<IActionResult> ReorderStops(int routeId, [FromBody] List<int> orderedStopIds)
        {
            var stops = await _context.RouteStops.Where(s => s.RouteId == routeId).ToListAsync();
            if (stops.Count != orderedStopIds.Count) return BadRequest("Stop count mismatch");
            
            for(int i = 0; i < orderedStopIds.Count; i++)
            {
                var stop = stops.FirstOrDefault(s => s.RouteStopId == orderedStopIds[i]);
                if (stop != null) stop.StopOrder = i + 1;
            }
            await _context.SaveChangesAsync();
            return Ok(stops);
        }
        
        [HttpPost("{routeId}/stops")]
        public async Task<IActionResult> AddStop(int routeId, [FromBody] RouteStopDto dto)
        {
            var stop = new RouteStop
            {
                RouteId = routeId,
                StopName = dto.StopName,
                StopOrder = dto.StopOrder,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DistanceFromStart = dto.DistanceFromStart,
                FareFromStart = dto.FareFromStart,
                MinutesFromDeparture = dto.MinutesFromDeparture
            };
            _context.RouteStops.Add(stop);
            await _context.SaveChangesAsync();
            return Ok(stop);
        }

        [HttpPut("stops/{stopId}")]
        public async Task<IActionResult> UpdateStop(int stopId, [FromBody] RouteStopDto dto)
        {
            var stop = await _context.RouteStops.FindAsync(stopId);
            if (stop == null) return NotFound();

            stop.StopName = dto.StopName;
            stop.Latitude = dto.Latitude;
            stop.Longitude = dto.Longitude;
            stop.DistanceFromStart = dto.DistanceFromStart;
            stop.FareFromStart = dto.FareFromStart;
            stop.MinutesFromDeparture = dto.MinutesFromDeparture;
            
            await _context.SaveChangesAsync();
            return Ok(stop);
        }
    }
}
