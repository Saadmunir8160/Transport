namespace AjkTransportSystem.API.Services
{
    public class AIService
    {
        // Smart Fare Prediction based on distance, time, and demand
        public static decimal PredictFare(decimal distance, DateTime travelDate, int seats)
        {
            decimal baseFare = distance * 3.5m;
            
            // Weekend surge pricing
            if (travelDate.DayOfWeek == DayOfWeek.Saturday || travelDate.DayOfWeek == DayOfWeek.Sunday)
            {
                baseFare *= 1.2m;
            }
            
            // Peak season (holidays)
            if (travelDate.Month == 12 || travelDate.Month == 7 || travelDate.Month == 8)
            {
                baseFare *= 1.15m;
            }
            
            // Group discount
            if (seats >= 5)
            {
                baseFare *= 0.9m;
            }
            
            return Math.Round(baseFare, 2);
        }

        // Route Recommendation based on distance and popularity
        public static string GetRouteRecommendation(decimal distance)
        {
            if (distance < 50)
                return "🚌 Short Journey - Perfect for quick trips!";
            else if (distance < 150)
                return "🛣️ Medium Journey - Comfortable travel experience!";
            else
                return "🌄 Long Journey - Scenic route with rest stops!";
        }

        // Travel Time Estimation
        public static string EstimateTravelTime(decimal distance)
        {
            decimal hours = distance / 60; // Average speed 60 km/h
            int totalMinutes = (int)(hours * 60);
            int hrs = totalMinutes / 60;
            int mins = totalMinutes % 60;
            
            return $"{hrs}h {mins}m";
        }

        // Best Time to Travel Suggestion
        public static string SuggestBestTime(DateTime travelDate)
        {
            if (travelDate.DayOfWeek == DayOfWeek.Saturday || travelDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return "⚠️ Weekend - Book early! High demand expected.";
            }
            else if (travelDate.DayOfWeek == DayOfWeek.Friday)
            {
                return "📅 Friday - Popular travel day. Book now!";
            }
            else
            {
                return "✅ Weekday - Good availability expected.";
            }
        }

        // Smart Route Matching
        public static int CalculateRouteScore(string routeName, string startPoint, string endPoint, string searchQuery)
        {
            int score = 0;
            searchQuery = searchQuery.ToLower();
            
            if (routeName.ToLower().Contains(searchQuery)) score += 10;
            if (startPoint.ToLower().Contains(searchQuery)) score += 8;
            if (endPoint.ToLower().Contains(searchQuery)) score += 8;
            
            return score;
        }
    }
}
