using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Services;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<backend.Models.Route> Routes { get; set; }
        public DbSet<RouteStop> RouteStops { get; set; }
        public DbSet<Trip> Trips { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(CreateTenantFilter(entityType.ClrType));
                }
            }
        }
        
        private LambdaExpression CreateTenantFilter(Type type)
        {
            var param = Expression.Parameter(type, "e");
            var property = Expression.Property(param, nameof(IMustHaveTenant.TenantId));
            
            var tenantIdValue = Expression.Property(Expression.Constant(this),
                typeof(AppDbContext).GetProperty(nameof(CurrentTenantId), BindingFlags.NonPublic | BindingFlags.Instance)!);
                
            var condition = Expression.Equal(property, Expression.Convert(tenantIdValue, typeof(Guid)));
            return Expression.Lambda(condition, param);
        }

        private Guid CurrentTenantId => _tenantProvider.GetTenantId() ?? Guid.Empty;
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().Where(e => e.State == EntityState.Added))
            {
                var tenantId = _tenantProvider.GetTenantId();
                if (tenantId.HasValue)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
                else
                {
                    throw new Exception("TenantId is required for this entity.");
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
