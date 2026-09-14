namespace backend.Models;

public class Booking : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public string PassengerId { get; set; } = string.Empty;
    public ApplicationUser? Passenger { get; set; }

    public int SeatNumber { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled

    public decimal Price { get; set; }
}
