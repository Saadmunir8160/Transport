namespace AjkTransportSystem.API.Services
{
    public class MountainDelayPredictionResult
    {
        public string RouteName { get; set; } = string.Empty;
        public string TerrainType { get; set; } = "Hilly Alpine";
        public string WeatherCondition { get; set; } = "Clear";
        public int BaseDurationMinutes { get; set; } = 120;
        public int PredictedDelayMinutes { get; set; }
        public int TotalEstimatedMinutes => BaseDurationMinutes + PredictedDelayMinutes;
        public string SeverityLevel { get; set; } = "Low"; // Low, Moderate, High, Critical
        public string Recommendation { get; set; } = string.Empty;
        public List<string> HazardFactors { get; set; } = new();
    }

    public static class AIWeatherPredictor
    {
        public static MountainDelayPredictionResult PredictRouteDelay(string startPoint, string endPoint, decimal distanceKm, string weatherCondition, bool isMonsoonOrWinter)
        {
            var result = new MountainDelayPredictionResult
            {
                RouteName = $"{startPoint} to {endPoint}",
                WeatherCondition = weatherCondition,
                BaseDurationMinutes = (int)(distanceKm * 1.5m) // Hilly average 40 km/h in AJK
            };

            int delay = 0;
            weatherCondition = weatherCondition?.ToLower() ?? "clear";

            // 1. Terrain & Altitude analysis for Azad Kashmir
            bool isSteepPass = (startPoint.Contains("Muzaffarabad") || startPoint.Contains("Rawalakot") || startPoint.Contains("Bagh") ||
                                endPoint.Contains("Muzaffarabad") || endPoint.Contains("Rawalakot") || endPoint.Contains("Bagh"));

            if (isSteepPass)
            {
                result.TerrainType = "High Altitude Mountain Pass";
                delay += 10;
                result.HazardFactors.Add("Steep hair-pin turns on Kohala / Azad Pattan sector (+10m)");
            }

            // 2. Weather & Hazard Dynamic Impact
            if (weatherCondition.Contains("snow") || weatherCondition.Contains("ice"))
            {
                delay += 45;
                result.HazardFactors.Add("Slippery snow conditions, reduced tire traction (+45m)");
                result.SeverityLevel = "High";
            }
            else if (weatherCondition.Contains("rain") || weatherCondition.Contains("storm"))
            {
                delay += 25;
                result.HazardFactors.Add("Active rainfall, high risk of micro-landslides near Kohala (+25m)");
                result.SeverityLevel = "Moderate";
            }
            else if (weatherCondition.Contains("fog") || weatherCondition.Contains("mist"))
            {
                delay += 20;
                result.HazardFactors.Add("Dense mountain fog, visibility under 50m (+20m)");
                result.SeverityLevel = "Moderate";
            }
            else
            {
                result.SeverityLevel = "Low";
                result.HazardFactors.Add("Clear mountain conditions, normal cruising speed");
            }

            // 3. Seasonal Surge
            if (isMonsoonOrWinter)
            {
                delay += 15;
                result.HazardFactors.Add("Seasonal weather advisory active in Northern AJK (+15m)");
            }

            result.PredictedDelayMinutes = delay;

            // 4. AI Strategic Passenger Advice
            if (delay > 35)
            {
                result.Recommendation = "⚠️ High Delay Advisory: Mountain pass speed restricted. Please keep 1-2 hours buffer time.";
            }
            else if (delay > 15)
            {
                result.Recommendation = "🌧️ Moderate Weather Advisory: Journey is safe, but drive cautiously due to wet asphalt.";
            }
            else
            {
                result.Recommendation = "🌟 Smooth Travel: Optimal road conditions detected for this route.";
            }

            return result;
        }
    }
}
