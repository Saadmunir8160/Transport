using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.DTOs;
using System.Security.Claims;

namespace AjkTransportSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 1; // Fallback for dev
        }

        // =======================
        // PROFILE
        // =======================

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Phone,
                user.LanguagePreference
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name = dto.Name ?? user.Name;
            user.Email = dto.Email ?? user.Email;
            user.Phone = dto.Phone ?? user.Phone;
            user.LanguagePreference = dto.LanguagePreference ?? user.LanguagePreference;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Mock verification
            if (user.PasswordHash != dto.OldPassword && !string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { message = "Incorrect old password." });
            }

            user.PasswordHash = dto.NewPassword;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Password updated successfully." });
        }

        // =======================
        // NOTIFICATIONS
        // =======================

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] string? type)
        {
            var userId = GetCurrentUserId();
            var query = _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.Type == type);
            }

            var notifications = await query.ToListAsync();
            return Ok(notifications);
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(notification);
        }

        [HttpPost("notifications/channels")]
        public IActionResult ChooseChannels([FromBody] dynamic payload)
        {
            // Mock endpoint for choosing channels (e.g. email vs sms)
            return Ok(new { message = "Channels updated successfully." });
        }
    }
}
