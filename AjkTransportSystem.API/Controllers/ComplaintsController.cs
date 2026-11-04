using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComplaintsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("passenger")]
        public async Task<IActionResult> GetPassengerComplaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Trip)
                .Include(c => c.Booking)
                .Where(c => c.Type == "Passenger")
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();
            return Ok(complaints);
        }

        [HttpGet("driver")]
        public async Task<IActionResult> GetDriverIssues()
        {
            var issues = await _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Trip)
                .Include(c => c.Vehicle)
                .Where(c => c.Type == "Driver")
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();
            return Ok(issues);
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveIssue(int id, [FromBody] ResolveIssueDto dto)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            if (dto.Status == "Resolved" && string.IsNullOrWhiteSpace(dto.ResolutionNote))
            {
                return BadRequest(new { message = "Resolving an issue requires a resolution note." });
            }

            complaint.Status = dto.Status;
            
            if (!string.IsNullOrWhiteSpace(dto.ResolutionNote))
            {
                complaint.AdminResponse = dto.ResolutionNote;
            }

            await _context.SaveChangesAsync();
            return Ok(complaint);
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetAttachments(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            // Mock attachment retrieval
            return Ok(new { url = complaint.PhotoUrl ?? "https://example.com/no-image.png" });
        }
    }
}
