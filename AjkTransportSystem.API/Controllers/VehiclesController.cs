using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;
using System.Security.Claims;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehiclesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int orgId)) return orgId;
            return 1;
        }

        private DateTime? CalculateNextExpiry(Vehicle vehicle)
        {
            var dates = new List<DateTime?>();
            if (vehicle.InsuranceExpiry.HasValue) dates.Add(vehicle.InsuranceExpiry);
            if (vehicle.FitnessExpiry.HasValue) dates.Add(vehicle.FitnessExpiry);
            if (vehicle.PermitExpiry.HasValue) dates.Add(vehicle.PermitExpiry);
            if (vehicle.PUCExpiry.HasValue) dates.Add(vehicle.PUCExpiry);
            
            return dates.Where(d => d.HasValue).Min();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orgId = GetCurrentOrganizationId();
            var vehicles = await _context.Vehicles.Where(v => v.OrganizationId == orgId).ToListAsync();
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleDto dto)
        {
            var orgId = GetCurrentOrganizationId();

            if (await _context.Vehicles.AnyAsync(v => v.OrganizationId == orgId && v.RegistrationNumber == dto.RegistrationNumber))
            {
                return BadRequest(new { message = "Registration number must be unique per organization." });
            }

            var vehicle = new Vehicle
            {
                OrganizationId = orgId,
                VehicleNumber = dto.RegistrationNumber, // map to old field if needed
                RegistrationNumber = dto.RegistrationNumber,
                VehicleType = dto.VehicleType,
                Capacity = dto.Capacity,
                Status = dto.Status,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                SeatLayout = dto.SeatLayout,
                FuelType = dto.FuelType,
                InsuranceExpiry = dto.InsuranceExpiry,
                FitnessExpiry = dto.FitnessExpiry,
                PermitExpiry = dto.PermitExpiry,
                PUCExpiry = dto.PUCExpiry
            };

            vehicle.NextDocumentExpiry = CalculateNextExpiry(vehicle);

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDto dto)
        {
            var orgId = GetCurrentOrganizationId();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id && v.OrganizationId == orgId);
            if (vehicle == null) return NotFound();

            // Capacity cannot drop below booked seats on future trips
            var maxBookedSeats = await _context.Trips
                .Where(t => t.VehicleId == id && t.TravelDate >= DateTime.UtcNow && t.Status != "Cancelled")
                .Select(t => (int?)t.BookedSeats)
                .MaxAsync() ?? 0;

            if (dto.Capacity < maxBookedSeats)
            {
                return BadRequest(new { message = $"Capacity cannot drop below {maxBookedSeats}, as seats are already booked on future trips." });
            }

            vehicle.VehicleType = dto.VehicleType;
            vehicle.Capacity = dto.Capacity;
            vehicle.Status = dto.Status;
            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.SeatLayout = dto.SeatLayout;
            vehicle.FuelType = dto.FuelType;
            vehicle.InsuranceExpiry = dto.InsuranceExpiry;
            vehicle.FitnessExpiry = dto.FitnessExpiry;
            vehicle.PermitExpiry = dto.PermitExpiry;
            vehicle.PUCExpiry = dto.PUCExpiry;
            vehicle.NextDocumentExpiry = CalculateNextExpiry(vehicle);

            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] dynamic payload)
        {
            var orgId = GetCurrentOrganizationId();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id && v.OrganizationId == orgId);
            if (vehicle == null) return NotFound();

            string status = payload.status;
            vehicle.Status = status;
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpGet("{id}/usage")]
        public async Task<IActionResult> GetUsageHistory(int id)
        {
            var orgId = GetCurrentOrganizationId();
            var trips = await _context.Trips
                .Include(t => t.Route)
                .Where(t => t.VehicleId == id && t.OrganizationId == orgId && t.Status == "Completed")
                .OrderByDescending(t => t.TravelDate)
                .ToListAsync();

            return Ok(trips);
        }
    }
}
