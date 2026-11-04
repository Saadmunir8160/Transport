using System;
using System.ComponentModel.DataAnnotations;

namespace AjkTransportSystem.API.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EntityType { get; set; } // "Vehicle" or "Driver"

        public int EntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; }

        public string? FileUrl { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string ReminderDays { get; set; } = "30,15,7"; // comma separated
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
