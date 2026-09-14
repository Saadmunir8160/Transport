using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Controllers;

[Authorize(Roles = "TransportAdmin")]
[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TripsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips()
    {
        var trips = await _context.Trips.ToListAsync();
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

    [HttpPost]
    public async Task<ActionResult<TripDto>> CreateTrip(CreateTripDto dto)
    {
        var trip = new Trip
        {
            RouteId = dto.RouteId,
            DriverId = dto.DriverId,
            VehicleId = dto.VehicleId,
            DepartureTime = dto.DepartureTime,
            EstimatedArrivalTime = dto.EstimatedArrivalTime,
            Status = "Scheduled"
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return Ok(new TripDto
        {
            Id = trip.Id,
            RouteId = trip.RouteId,
            DriverId = trip.DriverId,
            VehicleId = trip.VehicleId,
            DepartureTime = trip.DepartureTime,
            EstimatedArrivalTime = trip.EstimatedArrivalTime,
            Status = trip.Status
        });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTripStatus(Guid id, UpdateTripStatusDto dto)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        trip.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
