using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Services;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AIController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("predict-fare")]
        public IActionResult PredictFare(int routeId, string travelDate, int seats)
        {
            var route = _context.Routes.Find(routeId);
            if (route == null) return NotFound();

            var date = DateTime.Parse(travelDate);
            var predictedFare = AIService.PredictFare(route.Distance, date, seats);
            var recommendation = AIService.GetRouteRecommendation(route.Distance);
            var travelTime = AIService.EstimateTravelTime(route.Distance);
            var bestTime = AIService.SuggestBestTime(date);
            var weather = WeatherService.GetRouteWeather(route.StartPoint, route.EndPoint);

            return Ok(new
            {
                routeName = route.RouteName,
                distance = route.Distance,
                predictedFare = predictedFare,
                totalFare = predictedFare * seats,
                recommendation = recommendation,
                estimatedTime = travelTime,
                travelAdvice = bestTime,
                weather = weather
            });
        }

        [HttpGet("smart-search")]
        public async Task<IActionResult> SmartSearch(string query)
        {
            var routes = await _context.Routes.ToListAsync();
            
            var scoredRoutes = routes.Select(r => new
            {
                route = r,
                score = AIService.CalculateRouteScore(r.RouteName, r.StartPoint, r.EndPoint, query)
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Select(x => x.route)
            .ToList();

            return Ok(scoredRoutes);
        }

        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            var routes = await _context.Routes.OrderBy(r => r.Distance).Take(5).ToListAsync();
            
            var recommendations = routes.Select(r => new
            {
                routeId = r.RouteId,
                routeName = r.RouteName,
                startPoint = r.StartPoint,
                endPoint = r.EndPoint,
                distance = r.Distance,
                estimatedFare = AIService.PredictFare(r.Distance, DateTime.Now, 1),
                estimatedTime = AIService.EstimateTravelTime(r.Distance),
                recommendation = AIService.GetRouteRecommendation(r.Distance)
            }).ToList();

            return Ok(recommendations);
        }

        [HttpGet("mountain-delay-prediction")]
        public IActionResult PredictMountainDelay(string startPoint, string endPoint, decimal? distance, string? weather)
        {
            decimal dist = distance ?? 110m;
            string cond = weather ?? "Rainy";
            var now = DateTime.UtcNow;
            bool isWinterOrMonsoon = now.Month == 12 || now.Month == 1 || now.Month == 2 || now.Month == 7 || now.Month == 8;

            var prediction = AIWeatherPredictor.PredictRouteDelay(startPoint, endPoint, dist, cond, isWinterOrMonsoon);
            return Ok(prediction);
        }
    }
}
