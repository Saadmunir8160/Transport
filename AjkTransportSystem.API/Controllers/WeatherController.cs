using Microsoft.AspNetCore.Mvc;
using AjkTransportSystem.API.Services;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        [HttpGet("{city}")]
        public IActionResult GetWeather(string city)
        {
            var weather = WeatherService.GetWeather(city);
            return Ok(weather);
        }

        [HttpGet("route")]
        public IActionResult GetRouteWeather(string from, string to)
        {
            var routeWeather = WeatherService.GetRouteWeather(from, to);
            return Ok(routeWeather);
        }
    }
}
