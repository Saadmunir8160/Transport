using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FaresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFares()
        {
            var fares = await _context.Fares.ToListAsync();
            return Ok(fares);
        }

        [HttpGet("route/{routeId}")]
        public async Task<IActionResult> GetFaresByRoute(int routeId)
        {
            var fares = await _context.Fares
                .Where(f => f.RouteId == routeId)
                .ToListAsync();
            return Ok(fares);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFare(int id)
        {
            var fare = await _context.Fares.FindAsync(id);
            if (fare == null) return NotFound();
            return Ok(fare);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFare([FromBody] FareDto dto)
        {
            var fare = new Fare
            {
                RouteId = dto.RouteId,
                Amount = dto.Amount,
                PassengerType = dto.PassengerType
            };
            _context.Fares.Add(fare);
            await _context.SaveChangesAsync();
            return Ok(fare);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFare(int id, [FromBody] FareDto dto)
        {
            var fare = await _context.Fares.FindAsync(id);
            if (fare == null) return NotFound();

            fare.RouteId = dto.RouteId;
            fare.Amount = dto.Amount;
            fare.PassengerType = dto.PassengerType;

            await _context.SaveChangesAsync();
            return Ok(fare);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFare(int id)
        {
            var fare = await _context.Fares.FindAsync(id);
            if (fare == null) return NotFound();

            _context.Fares.Remove(fare);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Fare deleted" });
        }
    }
}
