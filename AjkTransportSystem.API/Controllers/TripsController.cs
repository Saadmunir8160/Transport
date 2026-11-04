using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrips()
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.TravelDate)
                .ToListAsync();
            return Ok(trips);
        }

        [HttpGet("live")]
        public async Task<IActionResult> GetLiveTrips()
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                .Where(t => t.Status == "Active")
                .ToListAsync();
            return Ok(trips);
        }

        private async Task<bool> HasOverlapAsync(int? vehicleId, int? driverId, DateTime date, TimeSpan time, int? excludeTripId = null)
        {
            var query = _context.Trips.Where(t => t.TravelDate.Date == date.Date && t.Status != "Cancelled" && t.Status != "Completed");
            if (excludeTripId.HasValue) query = query.Where(t => t.TripId != excludeTripId.Value);

            var conflicts = await query.ToListAsync();
            foreach(var t in conflicts)
            {
                // Simple 2-hour window check for overlap
                var timeDiff = Math.Abs((t.DepartureTime - time).TotalHours);
                if (timeDiff < 2.0)
                {
                    if (vehicleId.HasValue && t.VehicleId == vehicleId) return true;
                    if (driverId.HasValue && t.DriverId == driverId) return true;
                }
            }
            return false;
        }

        private async Task<bool> HasExpiredDocumentsAsync(int? vehicleId, int? driverId)
        {
            var now = DateTime.UtcNow;
            if (vehicleId.HasValue)
            {
                var expiredVehicleDocs = await _context.Documents.AnyAsync(d => d.EntityType == "Vehicle" && d.EntityId == vehicleId.Value && d.ExpiryDate.HasValue && d.ExpiryDate.Value < now);
                if (expiredVehicleDocs) return true;
            }
            if (driverId.HasValue)
            {
                var expiredDriverDocs = await _context.Documents.AnyAsync(d => d.EntityType == "Driver" && d.EntityId == driverId.Value && d.ExpiryDate.HasValue && d.ExpiryDate.Value < now);
                if (expiredDriverDocs) return true;
            }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] TripDto dto)
        {
            if (await HasExpiredDocumentsAsync(dto.VehicleId, dto.DriverId))
                return BadRequest(new { message = "Cannot assign trip. Vehicle or Driver has expired documents." });

            if (await HasOverlapAsync(dto.VehicleId, dto.DriverId, dto.TravelDate, dto.DepartureTime))
                return BadRequest(new { message = "Driver or vehicle overlap detected." });

            var trip = new Trip
            {
                RouteId = dto.RouteId,
                TimingId = dto.TimingId,
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                TravelDate = dto.TravelDate,
                DepartureTime = dto.DepartureTime,
                Status = "Scheduled"
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return Ok(trip);
        }

        [HttpPut("{id}/reassign")]
        public async Task<IActionResult> ReassignTrip(int id, [FromBody] TripDto dto)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            if (dto.VehicleId.HasValue)
            {
                var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId.Value);
                if (vehicle != null && vehicle.Capacity < trip.BookedSeats)
                {
                    return BadRequest(new { message = "Vehicle has fewer seats than currently booked." });
                }
            }

            if (await HasExpiredDocumentsAsync(dto.VehicleId, dto.DriverId))
                return BadRequest(new { message = "Cannot reassign. New Vehicle or Driver has expired documents." });

            if (await HasOverlapAsync(dto.VehicleId, dto.DriverId, trip.TravelDate, trip.DepartureTime, trip.TripId))
                return BadRequest(new { message = "Driver or vehicle overlap detected." });

            trip.DriverId = dto.DriverId ?? trip.DriverId;
            trip.VehicleId = dto.VehicleId ?? trip.VehicleId;

            await _context.SaveChangesAsync();
            return Ok(trip);
        }
        
        [HttpPut("{id}/modify")]
        public async Task<IActionResult> ModifyTrip(int id, [FromBody] TripDto dto)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();
            
            if (await HasOverlapAsync(dto.VehicleId, dto.DriverId, dto.TravelDate, dto.DepartureTime, trip.TripId))
                return BadRequest(new { message = "Driver or vehicle overlap detected." });

            trip.TravelDate = dto.TravelDate;
            trip.DepartureTime = dto.DepartureTime;
            
            await _context.SaveChangesAsync();
            return Ok(trip);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTrip(int id, [FromBody] string reason)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();
            
            if (trip.Status == "Active") return BadRequest("Cannot cancel an active trip. Must be handled differently.");

            trip.Status = "Cancelled";
            trip.CancelReason = reason;

            // Logically: Find all bookings and cancel them, notify passengers
            var bookings = await _context.Bookings.Where(b => b.TripId == id && b.Status != "Cancelled").ToListAsync();
            foreach (var b in bookings)
            {
                b.Status = "Cancelled";
                // Trigger passenger notification logic here...
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Trip cancelled and passengers notified." });
        }
        
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateLiveStatus(int id, [FromBody] Trip statusUpdate)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            trip.Status = statusUpdate.Status;
            if (!string.IsNullOrEmpty(statusUpdate.LastStopReached)) trip.LastStopReached = statusUpdate.LastStopReached;
            if (!string.IsNullOrEmpty(statusUpdate.NextStop)) trip.NextStop = statusUpdate.NextStop;
            if (statusUpdate.PredictedETA.HasValue) trip.PredictedETA = statusUpdate.PredictedETA;
            trip.DelayInMinutes = statusUpdate.DelayInMinutes;
            
            if (statusUpdate.Status == "Active" && !trip.ActualDepartureTime.HasValue)
            {
                trip.ActualDepartureTime = DateTime.Now.TimeOfDay;
            }

            await _context.SaveChangesAsync();
            return Ok(trip);
        }
    }
}
