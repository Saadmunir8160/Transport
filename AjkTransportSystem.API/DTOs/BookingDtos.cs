namespace AjkTransportSystem.API.DTOs
{
    public class MoveBookingDto
    {
        public int? NewTripId { get; set; }
        public string? NewSeatNumber { get; set; }
        public string AdminReason { get; set; } = string.Empty;
        public int AdminUserId { get; set; } // for audit log
    }

    public class CancelBookingDto
    {
        public string Reason { get; set; } = string.Empty;
        public int AdminUserId { get; set; } // for audit log
    }
}
