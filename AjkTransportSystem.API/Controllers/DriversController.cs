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
    public class DriversController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DriversController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int orgId)) return orgId;
            return 1;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orgId = GetCurrentOrganizationId();
            var drivers = await _context.Users
                .Where(u => u.Role == "Driver" && u.OrganizationId == orgId)
                .ToListAsync();
            return Ok(drivers);
        }

        [HttpPost]
        public async Task<IActionResult> AddDriver([FromBody] DriverDto dto)
        {
            var orgId = GetCurrentOrganizationId();

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email already in use." });
            }

            var user = new User
            {
                OrganizationId = orgId,
                Role = "Driver",
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                PasswordHash = dto.Password, // Mock hashing
                IDNumber = dto.IDNumber,
                Address = dto.Address,
                EmergencyContact = dto.EmergencyContact,
                PhotoUrl = dto.PhotoUrl,
                LicenseNumber = dto.LicenseNumber,
                LicenseType = dto.LicenseType,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                JoiningDate = dto.JoiningDate,
                AssignedVehicleId = dto.AssignedVehicleId,
                Status = dto.Status
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] DriverDto dto)
        {
            var orgId = GetCurrentOrganizationId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Driver" && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.IDNumber = dto.IDNumber;
            user.Address = dto.Address;
            user.EmergencyContact = dto.EmergencyContact;
            user.PhotoUrl = dto.PhotoUrl;
            user.LicenseNumber = dto.LicenseNumber;
            user.LicenseType = dto.LicenseType;
            user.LicenseIssueDate = dto.LicenseIssueDate;
            user.LicenseExpiryDate = dto.LicenseExpiryDate;
            user.JoiningDate = dto.JoiningDate;
            user.AssignedVehicleId = dto.AssignedVehicleId;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] dynamic payload)
        {
            var orgId = GetCurrentOrganizationId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Driver" && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            string status = payload.status;
            
            if (status == "Inactive")
            {
                // A driver with upcoming trips cannot be deactivated until reassigned
                var hasUpcomingTrips = await _context.Trips.AnyAsync(t => t.DriverId == id && t.TravelDate >= DateTime.UtcNow && t.Status != "Cancelled" && t.Status != "Completed");
                if (hasUpcomingTrips)
                {
                    return BadRequest(new { message = "Cannot deactivate. Driver has upcoming trips assigned." });
                }
            }

            user.Status = status;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetTripHistory(int id)
        {
            var orgId = GetCurrentOrganizationId();
            var trips = await _context.Trips
                .Include(t => t.Route)
                .Where(t => t.DriverId == id && t.OrganizationId == orgId && t.Status == "Completed")
                .OrderByDescending(t => t.TravelDate)
                .ToListAsync();

            return Ok(trips);
        }
    }
}
