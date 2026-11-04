namespace AjkTransportSystem.API.Models
{
    public class Fare
    {
        public int FareId { get; set; }
        public int RouteId { get; set; }
        public decimal Amount { get; set; }
        public string PassengerType { get; set; }
    }
}
