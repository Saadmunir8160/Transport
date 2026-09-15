using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Controllers;

[Authorize(Roles = "Driver")]
[ApiController]
[Route("api/[controller]")]
public class DriverAppController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DriverAppController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("trips")]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetMyTrips()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (driver == null) return NotFound("Driver profile not found.");

        var trips = await _context.Trips
            .Where(t => t.DriverId == driver.Id)
            .Include(t => t.Route)
            .Include(t => t.Vehicle)
            .ToListAsync();

        return Ok(trips.Select(t => new TripDto
        {
            Id = t.Id,
            RouteId = t.RouteId,
            DriverId = t.DriverId,
            VehicleId = t.VehicleId,
            DepartureTime = t.DepartureTime,
            EstimatedArrivalTime = t.EstimatedArrivalTime,
            Status = t.Status
        }));
    }

    [HttpPut("trips/{id}/status")]
    public async Task<IActionResult> UpdateTripStatus(Guid id, UpdateTripStatusDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (driver == null) return NotFound("Driver profile not found.");

        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id && t.DriverId == driver.Id);
        if (trip == null) return NotFound("Trip not found or not assigned to this driver.");

        trip.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
