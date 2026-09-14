using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return BadRequest(new { Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { Message = "User created successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user, userRoles);

                return Ok(new
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(120),
                    User = new { user.FirstName, user.LastName, user.Email, Roles = userRoles }
                });
            }
            return Unauthorized();
        }
    }
}
