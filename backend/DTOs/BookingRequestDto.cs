namespace backend.DTOs;

public class BookingRequestDto
{
    public Guid TripId { get; set; }
    public int SeatNumber { get; set; }
}
