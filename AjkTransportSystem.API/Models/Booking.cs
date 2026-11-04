namespace AjkTransportSystem.API.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int RouteId { get; set; }
        public int? TripId { get; set; }
        public string PassengerName { get; set; }
        public string PassengerPhone { get; set; }
        public string PassengerCNIC { get; set; }
        public int Seats { get; set; }
        public string? SeatNumber { get; set; } // e.g. "01, 02"
        public decimal TotalFare { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public string? CancelReason { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string TicketCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        public string? BoardingStop { get; set; }  // Where passenger boards
        public string? DroppingStop { get; set; }  // Where passenger gets off
        public bool IsNoShow { get; set; } = false;
        
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        
        public Route? Route { get; set; }
        public Trip? Trip { get; set; }
    }
}
