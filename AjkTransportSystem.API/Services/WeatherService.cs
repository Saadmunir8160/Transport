namespace AjkTransportSystem.API.Services
{
    public class WeatherService
    {
        // Mock weather data for AJK cities
        public static WeatherInfo GetWeather(string city)
        {
            var random = new Random();
            var weatherConditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Clear" };
            var icons = new[] { "☀️", "☁️", "🌧️", "⛅", "🌤️" };
            
            var temp = random.Next(15, 35);
            var condition = weatherConditions[random.Next(weatherConditions.Length)];
            var icon = icons[random.Next(icons.Length)];
            
            return new WeatherInfo
            {
                City = city,
                Temperature = temp,
                Condition = condition,
                Icon = icon,
                Humidity = random.Next(40, 80),
                WindSpeed = random.Next(5, 25),
                TravelAdvice = GetTravelAdvice(condition, temp)
            };
        }

        private static string GetTravelAdvice(string condition, int temp)
        {
            if (condition == "Rainy")
                return "⚠️ Rainy weather - Roads may be slippery. Travel with caution!";
            else if (condition == "Sunny" && temp > 30)
                return "🌡️ Hot weather - Stay hydrated during travel!";
            else if (temp < 18)
                return "🧥 Cool weather - Carry warm clothes!";
            else
                return "✅ Perfect weather for travel!";
        }

        public static RouteWeather GetRouteWeather(string startPoint, string endPoint)
        {
            return new RouteWeather
            {
                StartWeather = GetWeather(startPoint),
                EndWeather = GetWeather(endPoint),
                OverallAdvice = "Check weather conditions before travel"
            };
        }
    }

    public class WeatherInfo
    {
        public string City { get; set; } = "";
        public int Temperature { get; set; }
        public string Condition { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public string TravelAdvice { get; set; } = "";
    }

    public class RouteWeather
    {
        public WeatherInfo StartWeather { get; set; } = new();
        public WeatherInfo EndWeather { get; set; } = new();
        public string OverallAdvice { get; set; } = "";
    }
}
