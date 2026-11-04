using System;

namespace AjkTransportSystem.API.Models
{
    public class Trip
    {
        public int TripId { get; set; }
        public int RouteId { get; set; }
        public int? TimingId { get; set; } // Schedule reference
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        
        public DateTime TravelDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        
        public TimeSpan? ActualDepartureTime { get; set; }
        public TimeSpan? ActualArrivalTime { get; set; }
        
        public string Status { get; set; } = "Scheduled"; // Scheduled, Active, Completed, Cancelled
        public string? CompletedStops { get; set; } // Comma separated list of stop IDs or names
        
        // Live tracking & ETA fields
        public string? LastStopReached { get; set; }
        public string? NextStop { get; set; }
        public DateTime? PredictedETA { get; set; }
        public int DelayInMinutes { get; set; } = 0;
        
        // Booking & cancellation
        public int BookedSeats { get; set; } = 0;
        public string? CancelReason { get; set; }
        
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public Route? Route { get; set; }
        public Timing? Timing { get; set; }
        public User? Driver { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
