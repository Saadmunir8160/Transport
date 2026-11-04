using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjkTransportSystem.API.Models
{
    public class UserSession
    {
        [Key]
        public int SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        public string TokenHash { get; set; } // Hash of the JWT or a separate session token for security
        
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        
        public bool IsRevoked { get; set; } = false;
    }
}
