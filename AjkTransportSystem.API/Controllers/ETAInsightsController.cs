using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using System.Linq;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ETAInsightsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ETAInsightsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("predicted-vs-actual")]
        public async Task<IActionResult> GetPredictedVsActual([FromQuery] int? routeId)
        {
            var query = _context.Trips.Include(t => t.Route).Where(t => t.Status == "Completed" && t.ActualArrivalTime.HasValue && t.ActualDepartureTime.HasValue);
            
            if (routeId.HasValue) query = query.Where(t => t.RouteId == routeId.Value);

            var trips = await query.ToListAsync();

            if (!trips.Any())
            {
                // Show planned duration when a route has too little history
                var fallbackRoute = routeId.HasValue ? await _context.Routes.FindAsync(routeId.Value) : null;
                return Ok(new 
                {
                    InsufficientHistory = true,
                    PlannedDurationMinutes = fallbackRoute?.BaselineDuration ?? 120,
                    Message = "Too little history. Displaying planned baseline duration."
                });
            }

            var stats = trips
                .GroupBy(t => new { t.RouteId, t.Route.RouteName })
                .Select(g => new {
                    RouteId = g.Key.RouteId,
                    RouteName = g.Key.RouteName,
                    AverageActualDurationMinutes = g.Average(t => (t.ActualArrivalTime.Value - t.ActualDepartureTime.Value).TotalMinutes),
                    AveragePredictedDurationMinutes = g.Average(t => t.PredictedETA.HasValue ? (t.PredictedETA.Value.TimeOfDay - t.ActualDepartureTime.Value).TotalMinutes : t.Route.BaselineDuration),
                    TotalTripsAnalyzed = g.Count()
                });

            return Ok(stats);
        }

        [HttpGet("average-delay")]
        public async Task<IActionResult> GetAverageDelay()
        {
            var trips = await _context.Trips.Include(t => t.Route).Where(t => t.Status == "Completed").ToListAsync();
            var stats = trips
                .GroupBy(t => t.Route.RouteName)
                .Select(g => new {
                    Segment = g.Key,
                    AverageDelayMinutes = g.Average(t => t.DelayInMinutes)
                });

            return Ok(stats);
        }

        [HttpGet("delay-factors")]
        public async Task<IActionResult> GetDelayByFactors()
        {
            var trips = await _context.Trips.Where(t => t.Status == "Completed").ToListAsync();
            
            var byDayOfWeek = trips.GroupBy(t => t.TravelDate.DayOfWeek.ToString())
                                   .Select(g => new { Factor = g.Key, AvgDelay = g.Average(t => t.DelayInMinutes) });
            
            var byHourOfDay = trips.Where(t => t.ActualDepartureTime.HasValue)
                                   .GroupBy(t => t.ActualDepartureTime.Value.Hours.ToString() + ":00")
                                   .Select(g => new { Factor = g.Key, AvgDelay = g.Average(t => t.DelayInMinutes) });

            return Ok(new {
                ByDayOfWeek = byDayOfWeek,
                ByHourOfDay = byHourOfDay
            });
        }

        [HttpGet("model-accuracy")]
        public async Task<IActionResult> GetModelAccuracyScore()
        {
            var trips = await _context.Trips
                .Where(t => t.Status == "Completed" && t.PredictedETA.HasValue && t.ActualArrivalTime.HasValue)
                .ToListAsync();

            if (!trips.Any()) return Ok(new { Score = 0, Message = "Not enough data" });

            // Simple accuracy metric: error relative to total duration
            double totalErrorMinutes = 0;
            foreach(var t in trips)
            {
                totalErrorMinutes += Math.Abs((t.PredictedETA.Value.TimeOfDay - t.ActualArrivalTime.Value).TotalMinutes);
            }
            
            var avgError = totalErrorMinutes / trips.Count;
            var score = Math.Max(0, 100 - (avgError * 2)); // Artificial score calculation (1 minute error = -2%)

            return Ok(new {
                AccuracyScorePercentage = Math.Round(score, 2),
                AverageErrorMinutes = Math.Round(avgError, 2),
                TripsEvaluated = trips.Count
            });
        }
    }
}
