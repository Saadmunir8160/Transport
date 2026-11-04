using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTimings()
        {
            var timings = await _context.Timings.ToListAsync();
            return Ok(timings);
        }

        [HttpGet("route/{routeId}")]
        public async Task<IActionResult> GetTimingsByRoute(int routeId)
        {
            var timings = await _context.Timings
                .Where(t => t.RouteId == routeId)
                .ToListAsync();
            
            var result = new List<object>();
            foreach (var timing in timings)
            {
                var vehicle = await _context.Vehicles.FindAsync(timing.VehicleId);
                result.Add(new
                {
                    timingId = timing.TimingId,
                    routeId = timing.RouteId,
                    vehicleId = timing.VehicleId,
                    vehicleNumber = vehicle?.VehicleNumber,
                    vehicleType = vehicle?.VehicleType,
                    capacity = vehicle?.Capacity,
                    departureTime = timing.DepartureTime,
                    arrivalTime = timing.ArrivalTime,
                    operatingDays = timing.OperatingDays,
                    validFrom = timing.ValidFrom,
                    validTo = timing.ValidTo,
                    status = timing.Status,
                    defaultDriverId = timing.DefaultDriverId
                });
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTiming(int id)
        {
            var timing = await _context.Timings.FindAsync(id);
            if (timing == null) return NotFound();
            return Ok(timing);
        }

        private async Task<bool> IsOverlappingAsync(TimingDto dto, int? excludeId = null)
        {
            var query = _context.Timings.Where(t => t.Status == "Active" && 
                                                    (t.VehicleId == dto.VehicleId || 
                                                    (dto.DefaultDriverId != null && t.DefaultDriverId == dto.DefaultDriverId)));
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.TimingId != excludeId.Value);
            }

            var overlappingTimings = await query.ToListAsync();

            foreach(var t in overlappingTimings)
            {
                // Simple overlapping check based on times
                // A better approach would check operating days overlap as well
                if (dto.DepartureTime < t.ArrivalTime && dto.ArrivalTime > t.DepartureTime)
                {
                    // Check if days overlap
                    if (string.IsNullOrEmpty(dto.OperatingDays) || string.IsNullOrEmpty(t.OperatingDays)) return true;
                    var dtoDays = dto.OperatingDays.Split(',');
                    var tDays = t.OperatingDays.Split(',');
                    if (dtoDays.Intersect(tDays).Any()) return true;
                }
            }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTiming([FromBody] TimingDto dto)
        {
            try
            {
                if (await IsOverlappingAsync(dto)) return BadRequest(new { message = "Overlapping schedule with vehicle or driver detected." });

                var timing = new Timing
                {
                    RouteId = dto.RouteId,
                    VehicleId = dto.VehicleId,
                    DefaultDriverId = dto.DefaultDriverId,
                    DepartureTime = dto.DepartureTime,
                    ArrivalTime = dto.ArrivalTime,
                    OperatingDays = dto.OperatingDays,
                    ValidFrom = dto.ValidFrom,
                    ValidTo = dto.ValidTo,
                    Status = dto.Status ?? "Active"
                };
                _context.Timings.Add(timing);
                await _context.SaveChangesAsync();
                return Ok(timing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTiming(int id, [FromBody] TimingDto dto)
        {
            var timing = await _context.Timings.FindAsync(id);
            if (timing == null) return NotFound();
            
            if (await IsOverlappingAsync(dto, id)) return BadRequest(new { message = "Overlapping schedule with vehicle or driver detected." });

            timing.RouteId = dto.RouteId;
            timing.VehicleId = dto.VehicleId;
            timing.DefaultDriverId = dto.DefaultDriverId;
            timing.DepartureTime = dto.DepartureTime;
            timing.ArrivalTime = dto.ArrivalTime;
            timing.OperatingDays = dto.OperatingDays;
            timing.ValidFrom = dto.ValidFrom;
            timing.ValidTo = dto.ValidTo;
            
            // Edits affect future trips only. Not touching past trips here.
            
            await _context.SaveChangesAsync();
            return Ok(timing);
        }
        
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTimingStatus(int id, [FromBody] string status)
        {
            var timing = await _context.Timings.FindAsync(id);
            if (timing == null) return NotFound();
            timing.Status = status; // Active, Paused
            await _context.SaveChangesAsync();
            return Ok(timing);
        }
        
        [HttpPost("{id}/generate-trips")]
        public async Task<IActionResult> GenerateTrips(int id, [FromQuery] int days = 7)
        {
            var timing = await _context.Timings.FindAsync(id);
            if (timing == null || timing.Status != "Active") return BadRequest("Schedule not found or not active.");
            
            var generatedTrips = new List<Trip>();
            var today = DateTime.Today;
            
            var opsDays = string.IsNullOrEmpty(timing.OperatingDays) ? 
                new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" } : 
                timing.OperatingDays.Split(',');

            for (int i = 0; i < days; i++)
            {
                var targetDate = today.AddDays(i);
                
                // Check valid from/to
                if (timing.ValidFrom.HasValue && targetDate < timing.ValidFrom.Value.Date) continue;
                if (timing.ValidTo.HasValue && targetDate > timing.ValidTo.Value.Date) continue;
                
                var dayStr = targetDate.ToString("ddd");
                if (opsDays.Contains(dayStr))
                {
                    // Ensure trip doesn't exist already
                    var exists = await _context.Trips.AnyAsync(t => t.TimingId == id && t.TravelDate.Date == targetDate);
                    if (!exists)
                    {
                        var trip = new Trip
                        {
                            RouteId = timing.RouteId,
                            TimingId = timing.TimingId,
                            VehicleId = timing.VehicleId,
                            DriverId = timing.DefaultDriverId,
                            TravelDate = targetDate,
                            DepartureTime = timing.DepartureTime,
                            Status = "Scheduled"
                        };
                        _context.Trips.Add(trip);
                        generatedTrips.Add(trip);
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Generated {generatedTrips.Count} trips", trips = generatedTrips });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTiming(int id)
        {
            var timing = await _context.Timings.FindAsync(id);
            if (timing == null) return NotFound();

            _context.Timings.Remove(timing);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Timing deleted" });
        }
    }
}
