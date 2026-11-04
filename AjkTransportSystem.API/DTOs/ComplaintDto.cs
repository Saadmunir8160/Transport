using System;
using System.ComponentModel.DataAnnotations;

namespace AjkTransportSystem.API.DTOs
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? OrganizationId { get; set; }
        public int? TripId { get; set; }
        public int? BookingId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        public string PreferredResolution { get; set; }
        public string ContactMethodPreference { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string AdminResponse { get; set; }
    }

    public class CreateComplaintDto
    {
        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public int? TripId { get; set; }
        public int? BookingId { get; set; }

        [Required]
        [MinLength(20)]
        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        [StringLength(50)]
        public string PreferredResolution { get; set; }

        [StringLength(50)]
        public string ContactMethodPreference { get; set; }
    }
}
