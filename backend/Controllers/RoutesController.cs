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
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoutesController(AppDbContext context)
    {
        _context = context;
    }

    // Routes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutes()
    {
        var routes = await _context.Routes.ToListAsync();
        return Ok(routes.Select(r => new RouteDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        }));
    }

    [HttpPost]
    public async Task<ActionResult<RouteDto>> CreateRoute(CreateRouteDto dto)
    {
        var route = new backend.Models.Route
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return Ok(new RouteDto { Id = route.Id, Name = route.Name, Description = route.Description });
    }

    // Stops
    [HttpGet("stops")]
    public async Task<ActionResult<IEnumerable<StopDto>>> GetStops()
    {
        var stops = await _context.Stops.ToListAsync();
        return Ok(stops.Select(s => new StopDto
        {
            Id = s.Id,
            Name = s.Name,
            Location = s.Location
        }));
    }

    [HttpPost("stops")]
    public async Task<ActionResult<StopDto>> CreateStop(CreateStopDto dto)
    {
        var stop = new Stop
        {
            Name = dto.Name,
            Location = dto.Location
        };

        _context.Stops.Add(stop);
        await _context.SaveChangesAsync();

        return Ok(new StopDto { Id = stop.Id, Name = stop.Name, Location = stop.Location });
    }

    // RouteStops
    [HttpGet("{routeId}/stops")]
    public async Task<ActionResult<IEnumerable<RouteStopDto>>> GetRouteStops(Guid routeId)
    {
        var routeStops = await _context.RouteStops
            .Include(rs => rs.Stop)
            .Where(rs => rs.RouteId == routeId)
            .OrderBy(rs => rs.StopOrder)
            .ToListAsync();

        return Ok(routeStops.Select(rs => new RouteStopDto
        {
            Id = rs.Id,
            RouteId = rs.RouteId,
            StopId = rs.StopId,
            StopOrder = rs.StopOrder,
            Stop = new StopDto 
            { 
                Id = rs.Stop!.Id, 
                Name = rs.Stop.Name, 
                Location = rs.Stop.Location 
            }
        }));
    }

    [HttpPost("{routeId}/stops")]
    public async Task<ActionResult<RouteStopDto>> AddRouteStop(Guid routeId, CreateRouteStopDto dto)
    {
        if (routeId != dto.RouteId) return BadRequest();

        var routeStop = new RouteStop
        {
            RouteId = dto.RouteId,
            StopId = dto.StopId,
            StopOrder = dto.StopOrder
        };

        _context.RouteStops.Add(routeStop);
        await _context.SaveChangesAsync();

        return Ok(new RouteStopDto
        {
            Id = routeStop.Id,
            RouteId = routeStop.RouteId,
            StopId = routeStop.StopId,
            StopOrder = routeStop.StopOrder
        });
    }
}
