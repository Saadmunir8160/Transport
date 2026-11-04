using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteStopsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RouteStopsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("route/{routeId}")]
        public async Task<IActionResult> GetRouteStops(int routeId)
        {
            var stops = await _context.RouteStops
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StopOrder)
                .ToListAsync();

            return Ok(stops);
        }

        [HttpGet("calculate-fare")]
        public async Task<IActionResult> CalculateFare(int routeId, string fromStop, string toStop)
        {
            var stops = await _context.RouteStops
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StopOrder)
                .ToListAsync();

            var fromStopData = stops.FirstOrDefault(s => s.StopName.ToLower() == fromStop.ToLower());
            var toStopData = stops.FirstOrDefault(s => s.StopName.ToLower() == toStop.ToLower());

            if (fromStopData == null || toStopData == null)
            {
                return BadRequest(new { message = "Invalid stops" });
            }

            // Calculate fare between two stops
            var distance = toStopData.DistanceFromStart - fromStopData.DistanceFromStart;
            var fare = toStopData.FareFromStart - fromStopData.FareFromStart;

            return Ok(new
            {
                routeId = routeId,
                fromStop = fromStopData.StopName,
                toStop = toStopData.StopName,
                distance = distance,
                fare = fare,
                isDirect = fromStopData.StopOrder == 1 && toStopData.StopOrder == stops.Count
            });
        }

        [HttpGet("route/{routeId}/all-stops")]
        public async Task<IActionResult> GetRouteWithStops(int routeId)
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route == null) return NotFound();

            var stops = await _context.RouteStops
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StopOrder)
                .Select(rs => new
                {
                    rs.RouteStopId,
                    rs.StopName,
                    rs.StopOrder,
                    rs.DistanceFromStart,
                    rs.FareFromStart
                })
                .ToListAsync();

            return Ok(new
            {
                route = new
                {
                    route.RouteId,
                    route.RouteName,
                    route.StartPoint,
                    route.EndPoint,
                    route.Distance
                },
                stops = stops,
                totalStops = stops.Count,
                directFare = stops.LastOrDefault()?.FareFromStart ?? 0
            });
        }
    }
}
