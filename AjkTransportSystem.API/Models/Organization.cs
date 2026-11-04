using System;
using System.ComponentModel.DataAnnotations;

namespace AjkTransportSystem.API.Models
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        
        [Required]
        public string OrganizationCode { get; set; }
        
        [Required]
        public string OrganizationName { get; set; }
        
        public string Location { get; set; }
        public string District { get; set; }
        
        // e.g. "Active", "Inactive"
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public string? Subdomain { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsSoftDeleted { get; set; } = false;
        public int? AdminUserId { get; set; }

        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? OfficeHours { get; set; }
        public string? LogoUrl { get; set; }
        public string? Address { get; set; }
        public string? DefaultLanguage { get; set; } = "en";
    }
}
