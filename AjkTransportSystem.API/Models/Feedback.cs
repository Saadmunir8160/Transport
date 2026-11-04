namespace AjkTransportSystem.API.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
