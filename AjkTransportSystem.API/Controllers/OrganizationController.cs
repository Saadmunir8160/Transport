using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.DTOs;
using System.Security.Claims;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrganizationController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int orgId))
            {
                return orgId;
            }
            // Fallback for development if no token provided but authorized
            return 1;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var orgId = GetCurrentOrganizationId();
            var org = await _context.Organizations.FindAsync(orgId);
            if (org == null) return NotFound();

            return Ok(new
            {
                org.OrganizationId,
                org.OrganizationName,
                org.Location,
                org.District,
                org.Subdomain,
                org.Status,
                org.ContactPhone,
                org.ContactEmail,
                org.Address,
                org.LogoUrl,
                org.OfficeHours,
                org.DefaultLanguage
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] OrganizationUpdateDto dto)
        {
            var orgId = GetCurrentOrganizationId();
            var org = await _context.Organizations.FindAsync(orgId);
            if (org == null) return NotFound();

            // Editable by normal admin
            org.ContactPhone = dto.ContactPhone ?? org.ContactPhone;
            org.ContactEmail = dto.ContactEmail ?? org.ContactEmail;
            org.Address = dto.Address ?? org.Address;
            org.LogoUrl = dto.LogoUrl ?? org.LogoUrl;
            org.OfficeHours = dto.OfficeHours ?? org.OfficeHours;
            org.DefaultLanguage = dto.DefaultLanguage ?? org.DefaultLanguage;

            await _context.SaveChangesAsync();
            return Ok(org);
        }

        [HttpPut("super-admin/profile")]
        public async Task<IActionResult> UpdateSuperAdminFields([FromBody] dynamic payload)
        {
            // Dummy logic: In real app, verify Super Admin claim
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "SuperAdmin" && role != "TransportAdmin") // Allowing TransportAdmin for testing
            {
                return Forbid();
            }

            var orgId = GetCurrentOrganizationId();
            var org = await _context.Organizations.FindAsync(orgId);
            if (org == null) return NotFound();

            string status = payload.status;
            string subdomain = payload.subdomain;

            if (!string.IsNullOrEmpty(status)) org.Status = status;
            if (!string.IsNullOrEmpty(subdomain)) org.Subdomain = subdomain;

            await _context.SaveChangesAsync();
            return Ok(org);
        }
    }
}
