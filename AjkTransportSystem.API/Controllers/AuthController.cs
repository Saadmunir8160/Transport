using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.DTOs;
using AjkTransportSystem.API.Services;

namespace AjkTransportSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }


        [HttpPost("send-otp")]
        public IActionResult SendOtp(SendOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Phone number is required." });

            var user = _context.Users.FirstOrDefault(u => u.Phone == dto.Phone);
            if (user != null && user.Role != "Temp")
            {
                // This means a real user is registered with this phone
                return BadRequest(new { message = "This phone number is already registered." });
            }

            if (user == null)
            {
                user = new User
                {
                    Phone = dto.Phone,
                    Role = "Temp", // Temporary role just to store OTP
                    OTPAttempts = 0
                };
                _context.Users.Add(user);
            }

            if (user.OTPAttempts >= 3 && user.OTPExpiry.HasValue && user.OTPExpiry.Value > DateTime.UtcNow)
            {
                return BadRequest(new { message = "Maximum OTP attempts reached. Please wait 30 minutes." });
            }

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            
            // In a real app, send this via SMS API (Twilio, SNS, local aggregator)
            Console.WriteLine($"[SMS MOCK] Sending OTP {otp} to {dto.Phone}");

            user.PhoneOTP = otp;
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);
            
            // Reset attempts if lockout period passed
            if (user.OTPAttempts >= 3)
            {
                user.OTPAttempts = 0;
            }

            _context.SaveChanges();

            return Ok(new { message = "OTP sent successfully." });
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            // Email validation
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            // Check email format
            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "You have entered an invalid email address." });

            // Block fake/common emails
            if (IsFakeEmail(dto.Email))
                return BadRequest(new { message = "You have entered an invalid email address. Please use your personal email." });

            // Name validation
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3)
                return BadRequest(new { message = "Name must be at least 3 characters long." });

            // Password validation
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters long." });

            // Check if email already exists
            
            if (string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Phone number is required." });

            if (string.IsNullOrWhiteSpace(dto.OTP))
                return BadRequest(new { message = "OTP is required." });

            var tempUser = _context.Users.FirstOrDefault(u => u.Phone == dto.Phone && u.Role == "Temp");
            if (tempUser == null || tempUser.PhoneOTP != dto.OTP || tempUser.OTPExpiry < DateTime.UtcNow)
            {
                if (tempUser != null)
                {
                    tempUser.OTPAttempts++;
                    if (tempUser.OTPAttempts >= 3)
                    {
                        tempUser.OTPExpiry = DateTime.UtcNow.AddMinutes(30); // 30 min lockout
                    }
                    _context.SaveChanges();
                }
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            // Check if another REAL user has this phone
            if (_context.Users.Any(u => u.Phone == dto.Phone && u.Role != "Temp"))
                return BadRequest(new { message = "This phone number is already registered." });

            if (!string.IsNullOrWhiteSpace(dto.Email) && _context.Users.Any(u => u.Email != null && u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest(new { message = "This email is already registered. Please use a different email." });

            // SuperAdmin registration - BLOCKED
            if (dto.Role.ToLower() == "superadmin")
            {
                return BadRequest(new { message = "SuperAdmin registration is restricted. Contact system administrator." });
            }

            // Admin registration - BLOCKED
            if (dto.Role.ToLower() == "admin")
            {
                return BadRequest(new { message = "Admin registration is restricted. Contact system administrator." });
            }
            
            // Only Passenger registration allowed
            tempUser.Name = dto.Name.Trim();
            tempUser.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.ToLower().Trim();
            tempUser.Role = "Passenger";
            tempUser.PasswordHash = AuthService.HashPassword(dto.Password);
            tempUser.LanguagePreference = dto.LanguagePreference;
            tempUser.PhoneOTP = null;
            tempUser.OTPExpiry = null;
            tempUser.OTPAttempts = 0;
            
            _context.SaveChanges();

            return Ok(new { message = "Registration successful! You can now login." });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains("@") && email.Contains(".");
            }
            catch
            {
                return false;
            }
        }

        private bool IsFakeEmail(string email)
        {
            var lowerEmail = email.ToLower();
            
            // List of fake/test email patterns
            var fakePatterns = new[]
            {
                "example@", "test@", "demo@", "sample@", "fake@",
                "admin@example", "user@example", "info@example",
                "test@test", "demo@demo", "sample@sample",
                "@example.com", "@test.com", "@demo.com", "@sample.com"
            };

            // Check if email matches any fake pattern
            foreach (var pattern in fakePatterns)
            {
                if (lowerEmail.Contains(pattern))
                    return true;
            }

            // Block common fake usernames
            var fakeUsernames = new[] { "example", "test", "demo", "sample", "fake", "admin", "user" };
            var username = lowerEmail.Split('@')[0];
            
            if (fakeUsernames.Contains(username))
                return true;

            return false;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                var trimmedEmail = dto.Email.Trim().ToLower();
                _logger.LogInformation("Processing login request for user: {Email}", trimmedEmail);

                var user = _context.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == trimmedEmail);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for {Email}", trimmedEmail);
                    try
                    {
                        _context.AuditLogs.Add(new AuditLog
                        {
                            Action = "Login Failed",
                            EntityName = "User",
                            EntityId = trimmedEmail,
                            Description = $"Failed login attempt for unknown email {trimmedEmail}",
                            Timestamp = DateTime.UtcNow
                        });
                        _context.SaveChanges();
                    }
                    catch { }
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (string.IsNullOrEmpty(user.PasswordHash) || !AuthService.VerifyPassword(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Password mismatch for {Email}", trimmedEmail);
                    try
                    {
                        _context.AuditLogs.Add(new AuditLog
                        {
                            Action = "Login Failed",
                            EntityName = "User",
                            EntityId = user.UserId.ToString(),
                            Description = $"Failed login attempt for {user.Email}",
                            Timestamp = DateTime.UtcNow
                        });
                        _context.SaveChanges();
                    }
                    catch { }
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Check inactive organization
                if (user.OrganizationId != null)
                {
                    var org = _context.Organizations.FirstOrDefault(o => o.OrganizationId == user.OrganizationId);
                    if (org != null && org.Status != "Active")
                    {
                        return Unauthorized(new { message = "Your organization is currently inactive. Please contact support." });
                    }
                }

                var sessionGuid = Guid.NewGuid().ToString();
                var session = new UserSession
                {
                    UserId = user.UserId,
                    TokenHash = sessionGuid,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Device = Request.Headers["User-Agent"].ToString(),
                    LoginTime = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(1),
                    IsRevoked = false
                };

                try
                {
                    _context.UserSessions.Add(session);
                    _context.SaveChanges();

                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = "Login",
                        EntityName = "User",
                        EntityId = user.UserId.ToString(),
                        Description = $"User {user.Email} logged in successfully",
                        Timestamp = DateTime.UtcNow
                    });
                    _context.SaveChanges();
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Non-fatal error creating session or audit log for user: {Email}", trimmedEmail);
                }

                var tokenString = GenerateJwtToken(user, session.SessionId.ToString());
                _logger.LogInformation("Login successful for user {Email} with role {Role}", user.Email, user.Role);
                return Ok(new { token = tokenString, user = new { user.UserId, user.Name, user.Email, user.Role, user.OrganizationId, sessionId = session.SessionId } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login action: Type={ExceptionType}, Message={ExceptionMessage}, StackTrace={StackTrace}", ex.GetType().FullName, ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "Server error", message = ex.Message });
            }
        }

        private string GenerateJwtToken(User user, string sessionId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("SessionId", sessionId) // Embed session ID
            };

            if (user.OrganizationId.HasValue)
            {
                claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
            }

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddDays(1),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpDelete("delete-user/{email}")]
        public IActionResult DeleteUser(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpDelete("delete-all-admins")]
        public IActionResult DeleteAllAdmins()
        {
            var admins = _context.Users.Where(u => u.Role == "Admin").ToList();
            _context.Users.RemoveRange(admins);
            _context.SaveChanges();

            return Ok(new { message = $"{admins.Count} admin(s) deleted" });
        }

        [HttpDelete("delete-all-superadmins")]
        public IActionResult DeleteAllSuperAdmins()
        {
            var superAdmins = _context.Users.Where(u => u.Role == "SuperAdmin").ToList();
            _context.Users.RemoveRange(superAdmins);
            _context.SaveChanges();

            return Ok(new { message = $"{superAdmins.Count} superadmin(s) deleted" });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                u.Role
            }).ToList();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public IActionResult DeleteUserById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("create-admin")]
        public IActionResult CreateAdmin(RegisterDto dto)
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
                return BadRequest(new { message = "Invalid email address." });

            // Validate name
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3)
                return BadRequest(new { message = "Name must be at least 3 characters." });

            // Validate password
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters." });

            // Check if email exists
            
            if (string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Phone number is required." });

            if (string.IsNullOrWhiteSpace(dto.OTP))
                return BadRequest(new { message = "OTP is required." });

            var tempUser = _context.Users.FirstOrDefault(u => u.Phone == dto.Phone && u.Role == "Temp");
            if (tempUser == null || tempUser.PhoneOTP != dto.OTP || tempUser.OTPExpiry < DateTime.UtcNow)
            {
                if (tempUser != null)
                {
                    tempUser.OTPAttempts++;
                    if (tempUser.OTPAttempts >= 3)
                    {
                        tempUser.OTPExpiry = DateTime.UtcNow.AddMinutes(30); // 30 min lockout
                    }
                    _context.SaveChanges();
                }
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            // Check if another REAL user has this phone
            if (_context.Users.Any(u => u.Phone == dto.Phone && u.Role != "Temp"))
                return BadRequest(new { message = "This phone number is already registered." });

            if (!string.IsNullOrWhiteSpace(dto.Email) && _context.Users.Any(u => u.Email != null && u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest(new { message = "Email already exists." });

            var admin = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.ToLower().Trim(),
                Role = "Admin",
                PasswordHash = AuthService.HashPassword(dto.Password)
            };

            _context.Users.Add(admin);
            _context.SaveChanges();

            return Ok(new { message = "Admin account created successfully!" });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var sessionIdClaim = User.Claims.FirstOrDefault(c => c.Type == "SessionId");
            if (sessionIdClaim != null && int.TryParse(sessionIdClaim.Value, out int sessionId))
            {
                var session = _context.UserSessions.Find(sessionId);
                if (session != null)
                {
                    session.IsRevoked = true;
                    _context.SaveChanges();

                    // Log to AuditLog
                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = "Logout",
                        EntityName = "User",
                        EntityId = session.UserId.ToString(),
                        Description = $"User logged out manually and session was revoked.",
                        Timestamp = DateTime.UtcNow
                    });
                    _context.SaveChanges();
                }
            }

            return Ok(new { message = "Logged out successfully." });
        }
    }
}
