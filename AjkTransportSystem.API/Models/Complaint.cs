using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjkTransportSystem.API.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }

        public int? TripId { get; set; }
        [ForeignKey("TripId")]
        public Trip Trip { get; set; }

        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        public int? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "Passenger"; // Passenger, Driver

        [StringLength(20)]
        public string? Severity { get; set; } // Low, Medium, High (For driver issues)

        [Required]
        [StringLength(50)]
        public string Category { get; set; } // driver behaviour, vehicle condition, booking issue, route problem, payment issue, other

        [Required]
        [MinLength(20)]
        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        [StringLength(50)]
        public string PreferredResolution { get; set; } // refund, apology, other

        [StringLength(50)]
        public string ContactMethodPreference { get; set; } // SMS, email, in-app

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "open"; // open, in progress, resolved

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public string AdminResponse { get; set; }
    }
}
