using Microsoft.EntityFrameworkCore;
using AjkTransportSystem.API.Models;
using AjkTransportSystem.API.Services;

namespace AjkTransportSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public int? TenantId { get; set; }
        public bool IsSuperAdmin { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider = null) : base(options) 
        {
            TenantId = tenantProvider?.GetTenantId();
            IsSuperAdmin = tenantProvider?.IsSuperAdmin() ?? false;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Timing> Timings { get; set; }
        public DbSet<Fare> Fares { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RouteStop> RouteStops { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Advisory> Advisories { get; set; }
        
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filters for Tenant Isolation
            modelBuilder.Entity<User>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId || e.OrganizationId == null);
            modelBuilder.Entity<Vehicle>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Models.Route>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Trip>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Booking>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Timing>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<AuditLog>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Complaint>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
            modelBuilder.Entity<Advisory>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);

            // Fix decimal precision warnings
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalFare)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Fare>()
                .Property(f => f.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Models.Route>()
                .Property(r => r.Distance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Models.Route>()
                .Property(r => r.BaseFare)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RouteStop>()
                .Property(rs => rs.FareFromStart)
                .HasPrecision(18, 2);
        }
    }
}