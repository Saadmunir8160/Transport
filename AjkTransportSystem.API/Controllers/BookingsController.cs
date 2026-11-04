using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Route)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return Ok(bookings);
        }

        private async Task LogAudit(int adminId, string action, string entity, string entityId, string description)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminId,
                Action = action,
                EntityName = entity,
                EntityId = entityId,
                Description = description,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == "Cancelled") return BadRequest("Already cancelled");

            booking.Status = "Cancelled";
            booking.CancelReason = dto.Reason;

            // Restore seat availability
            if (booking.TripId.HasValue)
            {
                var trip = await _context.Trips.FindAsync(booking.TripId.Value);
                if (trip != null) trip.BookedSeats = Math.Max(0, trip.BookedSeats - booking.Seats);
            }

            await LogAudit(dto.AdminUserId, "Cancel", "Booking", id.ToString(), $"Cancelled with reason: {dto.Reason}");
            await _context.SaveChangesAsync();

            return Ok(booking);
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> MoveBooking(int id, [FromBody] MoveBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            var oldTripId = booking.TripId;
            var oldSeat = booking.SeatNumber;

            if (dto.NewTripId.HasValue && dto.NewTripId.Value != booking.TripId)
            {
                // Move logic: decrease seats in old, increase in new
                if (oldTripId.HasValue)
                {
                    var oldTrip = await _context.Trips.FindAsync(oldTripId.Value);
                    if (oldTrip != null) oldTrip.BookedSeats = Math.Max(0, oldTrip.BookedSeats - booking.Seats);
                }

                var newTrip = await _context.Trips.FindAsync(dto.NewTripId.Value);
                if (newTrip != null) newTrip.BookedSeats += booking.Seats;

                booking.TripId = dto.NewTripId.Value;
            }

            if (!string.IsNullOrEmpty(dto.NewSeatNumber))
            {
                booking.SeatNumber = dto.NewSeatNumber;
            }

            await LogAudit(dto.AdminUserId, "Move", "Booking", id.ToString(), $"Moved. Trip {oldTripId}->{booking.TripId}, Seat {oldSeat}->{booking.SeatNumber}. Reason: {dto.AdminReason}");
            await _context.SaveChangesAsync();

            return Ok(booking);
        }

        [HttpGet("{id}/export")]
        public IActionResult ExportBooking(int id)
        {
            // In a real scenario, this would return a FileContentResult (PDF or Excel byte array)
            // Here, we just mock the response url or base64.
            return Ok(new { url = $"https://example.com/exports/booking_{id}.pdf" });
        }
    }
}
