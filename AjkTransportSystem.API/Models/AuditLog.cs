using System;

namespace AjkTransportSystem.API.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public int? OrganizationId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
    }
}
