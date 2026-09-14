using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PassengerController : ControllerBase
{
    private readonly AppDbContext _context;

    public PassengerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("trips/search")]
    public async Task<IActionResult> SearchTrips([FromQuery] Guid originId, [FromQuery] Guid destinationId, [FromQuery] DateTime date)
    {
        // Simple search: find trips where route has both origin and destination stops, and origin comes before destination
        // For Phase 5, we will return trips on that date.
        var targetDate = date.Date;

        var trips = await _context.Trips
            .Include(t => t.Route)
            .ThenInclude(r => r.RouteStops)
            .Include(t => t.Vehicle)
            .Where(t => t.DepartureTime.Date == targetDate && t.Status != "Cancelled")
            .ToListAsync();

        var validTrips = trips.Where(t => 
            t.Route != null && 
            t.Route.RouteStops.Any(rs => rs.StopId == originId) && 
            t.Route.RouteStops.Any(rs => rs.StopId == destinationId) &&
            t.Route.RouteStops.First(rs => rs.StopId == originId).StopOrder < t.Route.RouteStops.First(rs => rs.StopId == destinationId).StopOrder
        ).Select(t => new {
            t.Id,
            RouteName = t.Route?.Name,
            t.DepartureTime,
            t.EstimatedArrivalTime,
            t.Price,
            VehicleType = t.Vehicle?.Type,
            Capacity = t.Vehicle?.Capacity
        });

        return Ok(validTrips);
    }

    [HttpGet("trips/{id}/seats")]
    public async Task<IActionResult> GetTripSeats(Guid id)
    {
        var trip = await _context.Trips.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == id);
        if (trip == null) return NotFound("Trip not found.");

        var occupiedSeats = await _context.Bookings
            .Where(b => b.TripId == id && b.Status == "Confirmed")
            .Select(b => b.SeatNumber)
            .ToListAsync();

        return Ok(new {
            Capacity = trip.Vehicle?.Capacity ?? 0,
            OccupiedSeats = occupiedSeats
        });
    }

    [Authorize(Roles = "Passenger")]
    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var trip = await _context.Trips.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == request.TripId);
        if (trip == null) return NotFound("Trip not found");
        
        if (request.SeatNumber <= 0 || request.SeatNumber > trip.Vehicle?.Capacity)
            return BadRequest("Invalid seat number");

        var booking = new Booking
        {
            TripId = request.TripId,
            PassengerId = userId,
            SeatNumber = request.SeatNumber,
            Price = trip.Price
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Booking successful", BookingId = booking.Id });
        }
        catch (DbUpdateException)
        {
            // Unique constraint on (TripId, SeatNumber) handles double booking
            return Conflict("This seat has already been booked. Please choose another seat.");
        }
    }

    [Authorize(Roles = "Passenger")]
    [HttpGet("bookings/my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var bookings = await _context.Bookings
            .Include(b => b.Trip)
            .ThenInclude(t => t.Route)
            .Where(b => b.PassengerId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new {
                b.Id,
                b.SeatNumber,
                b.BookingDate,
                b.Status,
                b.Price,
                RouteName = b.Trip.Route.Name,
                DepartureTime = b.Trip.DepartureTime
            })
            .ToListAsync();

        return Ok(bookings);
    }
}
