using backend.Data;
using backend.Models;
using backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("superadmin-stats")]
        public async Task<IActionResult> GetSuperAdminStats()
        {
            var totalOrganizations = await _context.Organizations.CountAsync();
            var totalUsers = await _userManager.Users.CountAsync();
            
            // Note: In Identity, to count users by role accurately, we often need a join or GetUsersInRoleAsync
            // For simple stats, we can just get all TransportAdmins
            var transportAdmins = await _userManager.GetUsersInRoleAsync("TransportAdmin");
            var totalTransportAdmins = transportAdmins.Count;

            return Ok(new DashboardStatsDto
            {
                TotalOrganizations = totalOrganizations,
                TotalUsers = totalUsers,
                TotalTransportAdmins = totalTransportAdmins
            });
        }
    }
}
