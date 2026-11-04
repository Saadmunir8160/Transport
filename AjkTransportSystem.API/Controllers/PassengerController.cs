using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/passenger")]
    [ApiController]
    [Authorize(Roles = "Passenger")]
    public class PassengerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PassengerController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new Exception("User ID not found in token.");
        }

        [HttpGet("trips/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchTrips([FromQuery] string startPoint, [FromQuery] string endPoint, [FromQuery] DateTime travelDate)
        {
            try
            {
                var tripsQuery = _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Status == "Scheduled" || t.Status == "Active");

                if (!string.IsNullOrEmpty(startPoint))
                {
                    tripsQuery = tripsQuery.Where(t => t.Route.StartPoint.Contains(startPoint));
                }

                if (!string.IsNullOrEmpty(endPoint))
                {
                    tripsQuery = tripsQuery.Where(t => t.Route.EndPoint.Contains(endPoint));
                }

                if (travelDate != default)
                {
                    tripsQuery = tripsQuery.Where(t => t.TravelDate.Date == travelDate.Date);
                }

                var trips = await tripsQuery
                    .Select(t => new {
                        t.TripId,
                        t.TravelDate,
                        t.DepartureTime,
                        t.Status,
                        RouteName = t.Route.RouteName,
                        StartPoint = t.Route.StartPoint,
                        EndPoint = t.Route.EndPoint,
                        VehicleType = t.Vehicle.VehicleType,
                        TotalCapacity = t.Vehicle.Capacity,
                        BookedSeatsCount = _context.Bookings.Where(b => b.TripId == t.TripId && b.Status == "Confirmed").Sum(b => (int?)b.Seats) ?? 0,
                        Fare = _context.Fares.Where(f => f.RouteId == t.RouteId).Select(f => f.Amount).FirstOrDefault() // Basic fare fallback
                    })
                    .ToListAsync();

                // Calculate available seats
                var tripsWithAvailability = trips.Select(t => new {
                    t.TripId, t.TravelDate, t.DepartureTime, t.Status,
                    t.RouteName, t.StartPoint, t.EndPoint,
                    t.VehicleType, t.Fare,
                    AvailableSeats = t.TotalCapacity - t.BookedSeatsCount
                }).Where(t => t.AvailableSeats > 0).ToList();

                return Ok(tripsWithAvailability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("trips/{id}/seats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripSeats(int id)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.TripId == id);
                    
                if (trip == null) return NotFound(new { message = "Trip not found." });

                var bookedSeatsStr = await _context.Bookings
                    .Where(b => b.TripId == id && b.Status == "Confirmed" && b.SeatNumber != null)
                    .Select(b => b.SeatNumber)
                    .ToListAsync();
                
                // Aggregate all comma-separated booked seats
                var allBookedSeats = new List<string>();
                foreach (var seatStr in bookedSeatsStr)
                {
                    if (!string.IsNullOrEmpty(seatStr))
                    {
                        var seats = seatStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                        allBookedSeats.AddRange(seats);
                    }
                }

                return Ok(new {
                    totalCapacity = trip.Vehicle.Capacity,
                    bookedSeats = allBookedSeats.Distinct().ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class CreateBookingDto
        {
            public int TripId { get; set; }
            public string PassengerName { get; set; }
            public string PassengerPhone { get; set; }
            public string PassengerCNIC { get; set; }
            public List<string> SelectedSeats { get; set; }
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> BookTrip([FromBody] CreateBookingDto dto)
        {
            if (dto.SelectedSeats == null || dto.SelectedSeats.Count == 0)
            {
                return BadRequest(new { message = "At least one seat must be selected." });
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = _context.Database.IsSqlServer() 
                    ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : await _context.Database.BeginTransactionAsync();
                try
                {
                    var userId = GetCurrentUserId();

                    // In SQL Server, lock the Trip row exclusively (UPDLOCK, ROWLOCK) to serialize concurrent bookings for the trip
                    if (_context.Database.IsSqlServer())
                    {
                        await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM Trips WITH (UPDLOCK, ROWLOCK) WHERE TripId = {0}", dto.TripId);
                    }

                    var trip = await _context.Trips
                        .Include(t => t.Route)
                        .Include(t => t.Vehicle)
                        .FirstOrDefaultAsync(t => t.TripId == dto.TripId);
                        
                    if (trip == null) return NotFound(new { message = "Trip not found." });

                    // Capacity integrity check: cannot exceed vehicle capacity
                    int vehicleCapacity = trip.Vehicle?.Capacity ?? 40;
                    if (trip.BookedSeats + dto.SelectedSeats.Count > vehicleCapacity)
                    {
                        return BadRequest(new { message = "Cannot exceed vehicle maximum capacity." });
                    }

                    // Check for already booked seats to prevent race conditions & double-booking
                    var existingBookedSeats = await _context.Bookings
                        .Where(b => b.TripId == dto.TripId && b.Status == "Confirmed" && b.SeatNumber != null)
                        .Select(b => b.SeatNumber)
                        .ToListAsync();

                    var occupiedSeats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var seatStr in existingBookedSeats)
                    {
                        if (!string.IsNullOrEmpty(seatStr))
                        {
                            foreach (var s in seatStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                occupiedSeats.Add(s.Trim());
                            }
                        }
                    }

                    var conflictingSeats = dto.SelectedSeats.Where(s => occupiedSeats.Contains(s.Trim())).ToList();
                    if (conflictingSeats.Any())
                    {
                        return BadRequest(new { 
                            message = $"Seats already booked: {string.Join(", ", conflictingSeats)}. Please select different seats." 
                        });
                    }

                    var fareAmount = await _context.Fares
                        .Where(f => f.RouteId == trip.RouteId)
                        .Select(f => f.Amount)
                        .FirstOrDefaultAsync();
                        
                    var totalFare = fareAmount * dto.SelectedSeats.Count;

                    var booking = new Booking
                    {
                        UserId = userId,
                        RouteId = trip.RouteId,
                        TripId = trip.TripId,
                        OrganizationId = trip.OrganizationId,
                        PassengerName = dto.PassengerName,
                        PassengerPhone = dto.PassengerPhone,
                        PassengerCNIC = dto.PassengerCNIC,
                        Seats = dto.SelectedSeats.Count,
                        SeatNumber = string.Join(",", dto.SelectedSeats),
                        TotalFare = totalFare,
                        TravelDate = trip.TravelDate,
                        BookingDate = DateTime.UtcNow,
                        Status = "Confirmed"
                    };

                    // Keep trip BookedSeats in sync
                    trip.BookedSeats += dto.SelectedSeats.Count;

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Booking successful", bookingId = booking.BookingId, totalFare });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message });
                }
            });
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookings = await _context.Bookings
                    .Include(b => b.Route)
                    .Where(b => b.UserId == userId)
                    .Select(b => new {
                        b.BookingId,
                        b.TravelDate,
                        b.BookingDate,
                        b.Status,
                        b.PassengerName,
                        b.Seats,
                        b.SeatNumber,
                        b.TotalFare,
                        RouteName = b.Route.RouteName,
                        StartPoint = b.Route.StartPoint,
                        EndPoint = b.Route.EndPoint
                    })
                    .OrderByDescending(b => b.TravelDate)
                    .ToListAsync();
                    
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("bookings/{id}/ticket")]
        public async Task<IActionResult> GetTicket(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _context.Bookings
                    .Include(b => b.Route)
                    .Include(b => b.Trip)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);
                    
                if (booking == null) return NotFound(new { message = "Booking not found or access denied." });

                var ticket = new {
                    bookingId = booking.BookingId,
                    passengerName = booking.PassengerName,
                    passengerPhone = booking.PassengerPhone,
                    passengerCNIC = booking.PassengerCNIC,
                    routeName = booking.Route.RouteName,
                    startPoint = booking.Route.StartPoint,
                    endPoint = booking.Route.EndPoint,
                    travelDate = booking.TravelDate,
                    departureTime = booking.Trip?.DepartureTime,
                    bookingDate = booking.BookingDate,
                    seats = booking.Seats,
                    seatNumber = booking.SeatNumber,
                    totalFare = booking.TotalFare,
                    status = booking.Status,
                    qrCode = $"AJK-{booking.BookingId}-{booking.PassengerCNIC}"
                };

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = _context.Database.IsSqlServer()
                    ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : await _context.Database.BeginTransactionAsync();
                try
                {
                    var userId = GetCurrentUserId();

                    // In SQL Server, acquire an exclusive lock on the booking row to serialize concurrent cancellations
                    if (_context.Database.IsSqlServer())
                    {
                        await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM Bookings WITH (UPDLOCK, ROWLOCK) WHERE BookingId = {0}", id);
                    }

                    var booking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);
                        
                    if (booking == null) return NotFound(new { message = "Booking not found or access denied." });

                    if (booking.Status == "Cancelled")
                        return BadRequest(new { message = "Booking is already cancelled." });

                    booking.Status = "Cancelled";

                    // Free up the seats on the trip exactly once
                    if (booking.TripId.HasValue)
                    {
                        var trip = await _context.Trips.FindAsync(booking.TripId.Value);
                        if (trip != null)
                        {
                            trip.BookedSeats = Math.Max(0, trip.BookedSeats - booking.Seats);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Booking cancelled successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message });
                }
            });
        }
    }
}
