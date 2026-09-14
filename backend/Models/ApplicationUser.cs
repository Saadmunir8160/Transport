using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        // Null for SuperAdmin, set for other roles
        public Guid? TenantId { get; set; }
    }
}
