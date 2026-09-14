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
public class DriversController : ControllerBase
{
    private readonly AppDbContext _context;

    public DriversController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetDrivers()
    {
        var drivers = await _context.Drivers.ToListAsync();
        return Ok(drivers.Select(d => new DriverDto
        {
            Id = d.Id,
            Name = d.Name,
            LicenseNumber = d.LicenseNumber,
            LicenseExpiryDate = d.LicenseExpiryDate,
            ContactNumber = d.ContactNumber
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDto>> GetDriver(Guid id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null) return NotFound();

        return Ok(new DriverDto
        {
            Id = driver.Id,
            Name = driver.Name,
            LicenseNumber = driver.LicenseNumber,
            LicenseExpiryDate = driver.LicenseExpiryDate,
            ContactNumber = driver.ContactNumber
        });
    }

    [HttpPost]
    public async Task<ActionResult<DriverDto>> CreateDriver(CreateDriverDto dto)
    {
        var driver = new Driver
        {
            Name = dto.Name,
            LicenseNumber = dto.LicenseNumber,
            LicenseExpiryDate = dto.LicenseExpiryDate,
            ContactNumber = dto.ContactNumber
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDriver), new { id = driver.Id }, new DriverDto
        {
            Id = driver.Id,
            Name = driver.Name,
            LicenseNumber = driver.LicenseNumber,
            LicenseExpiryDate = driver.LicenseExpiryDate,
            ContactNumber = driver.ContactNumber
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriver(Guid id, UpdateDriverDto dto)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null) return NotFound();

        driver.Name = dto.Name;
        driver.LicenseNumber = dto.LicenseNumber;
        driver.LicenseExpiryDate = dto.LicenseExpiryDate;
        driver.ContactNumber = dto.ContactNumber;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(Guid id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null) return NotFound();

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
