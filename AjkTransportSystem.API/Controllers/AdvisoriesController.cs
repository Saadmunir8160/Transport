using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdvisoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdvisories()
        {
            var advisories = await _context.Advisories.ToListAsync();
            return Ok(advisories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdvisory([FromBody] AdvisoryDto dto)
        {
            var advisory = new Advisory
            {
                RoadOrSegment = dto.RoadOrSegment,
                Reason = dto.Reason,
                Status = dto.Status,
                StartedAt = dto.StartedAt,
                ExpectedReopening = dto.ExpectedReopening,
                AffectedRoutes = dto.AffectedRoutes,
                Source = dto.Source
            };
            
            _context.Advisories.Add(advisory);
            await _context.SaveChangesAsync();
            return Ok(advisory);
        }

        [HttpPut("{id}/close")]
        public async Task<IActionResult> CloseAdvisory(int id)
        {
            var advisory = await _context.Advisories.FindAsync(id);
            if (advisory == null) return NotFound();
            
            advisory.Status = "Closed";
            await _context.SaveChangesAsync();
            return Ok(advisory);
        }
    }
}
