using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Entities.Seedworks;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using System.Text.Json;

namespace NetCoreTemplate.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserProfile> AppUserProfiles => Set<AppUserProfile>();
    public DbSet<AppUserRefreshToken> AppUserRefreshTokens => Set<AppUserRefreshToken>();
    public DbSet<AppUserActivited> AppUserActivities => Set<AppUserActivited>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ProcessAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ProcessAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ProcessAudit()
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();

        Guid? currentUserId = null;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedDate)).CurrentValue = DateTime.UtcNow;
                    currentUserId ??= (Guid?)entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.ModifiedDate)).CurrentValue = DateTime.UtcNow;
                    currentUserId ??= (Guid?)entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(BaseEntity.Status)).CurrentValue = EntityStatus.Deleted;
                    entry.Property(nameof(BaseEntity.DeletedDate)).CurrentValue = DateTime.UtcNow;
                    currentUserId ??= (Guid?)entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue;
                    break;
            }
        }

        var activityLogs = new List<AppUserActivited>();

        foreach (var entry in entries)
        {
            var type = entry.Entity.GetType();
            if (type == typeof(AppUserActivited) || type == typeof(SystemLog)) continue;

            var entityId = (Guid)entry.Property(nameof(BaseEntity.Id)).CurrentValue!;
            var entityName = type.Name;

            UserActivityType activityType;
            string description;

            if (entry.State == EntityState.Added)
            {
                activityType = UserActivityType.EntityCreated;
                description = $"Created new {entityName}";
            }
            else if (entry.State == EntityState.Modified)
            {
                activityType = UserActivityType.EntityUpdated;
                var changes = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                    {
                        changes[prop.Metadata.Name] = new { Original = prop.OriginalValue, Current = prop.CurrentValue };
                    }
                }
                description = changes.Count > 0 ? $"Updated {entityName}: " + JsonSerializer.Serialize(changes) : $"Updated {entityName}";
            }
            else if ((EntityStatus)entry.Property(nameof(BaseEntity.Status)).CurrentValue! == EntityStatus.Deleted && entry.State == EntityState.Modified)
            {
                activityType = UserActivityType.EntityDeleted;
                description = $"Deleted (soft) {entityName}";
            }
            else continue;

            var logUserId = entry.State == EntityState.Deleted
                ? (Guid?)entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue
                : entry.State == EntityState.Modified
                    ? (Guid?)entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue
                    : (Guid?)entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue;

            activityLogs.Add(new AppUserActivited(logUserId, activityType, description, entityName, entityId));
        }

        if (activityLogs.Count > 0) AddRange(activityLogs);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var param = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var statusProperty = System.Linq.Expressions.Expression.Property(param, nameof(BaseEntity.Status));
            var deletedValue = System.Linq.Expressions.Expression.Constant(EntityStatus.Deleted);
            var notEqual = System.Linq.Expressions.Expression.NotEqual(statusProperty, deletedValue);
            var lambda = System.Linq.Expressions.Expression.Lambda(notEqual, param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
