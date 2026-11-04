namespace AjkTransportSystem.API.DTOs
{
    public class BookingDto
    {
        public int UserId { get; set; }
        public int RouteId { get; set; }
        public string PassengerName { get; set; }
        public string PassengerPhone { get; set; }
        public string PassengerCNIC { get; set; }
        public int Seats { get; set; }
        public decimal TotalFare { get; set; }
        public DateTime TravelDate { get; set; }
    }
}
