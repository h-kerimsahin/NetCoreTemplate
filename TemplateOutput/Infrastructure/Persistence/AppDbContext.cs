using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Entities.Seedworks;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace $safeprojectname$.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _auditProcessed;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserProfile> AppUserProfiles => Set<AppUserProfile>();
    public DbSet<AppUserRefreshToken> AppUserRefreshTokens => Set<AppUserRefreshToken>();
    public DbSet<AppUserActivited> AppUserActivities => Set<AppUserActivited>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<BackgroundJobLog> BackgroundJobLogs => Set<BackgroundJobLog>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();
    public DbSet<AppPermission> AppPermissions => Set<AppPermission>();
    public DbSet<AppRolePermission> AppRolePermissions => Set<AppRolePermission>();

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _auditProcessed = false;
        ProcessAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        _auditProcessed = false;
        ProcessAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ProcessAudit()
    {
        if (_auditProcessed) return;
        _auditProcessed = true;

        var entries = ChangeTracker.Entries<BaseEntity>().ToList();

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? currentUserId = string.IsNullOrWhiteSpace(userId) ? null : Guid.Parse(userId);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedDate)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.ModifiedDate)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.ModifiedBy)).CurrentValue = currentUserId;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(BaseEntity.Status)).CurrentValue = EntityStatus.Deleted;
                    entry.Property(nameof(BaseEntity.DeletedDate)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue = currentUserId;
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

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in entries)
        {
            var type = entry.Entity.GetType();
            if (type == typeof(AuditEntry)) continue;

            var entityId = (Guid)entry.Property(nameof(BaseEntity.Id)).CurrentValue!;
            var entityName = type.Name;
            byte entityState;

            if (entry.State == EntityState.Added)
            {
                entityState = 1;
            }
            else if (entry.State == EntityState.Modified)
            {
                entityState = 2;
            }
            else if ((EntityStatus)entry.Property(nameof(BaseEntity.Status)).CurrentValue! == EntityStatus.Deleted && entry.State == EntityState.Modified)
            {
                entityState = 3;
            }
            else continue;

            if (entry.State == EntityState.Added)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    auditEntries.Add(AuditEntry.Create(
                        entityName,
                        entityId,
                        prop.Metadata.Name,
                        null,
                        prop.CurrentValue?.ToString(),
                        currentUserId,
                        entityState));
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                    {
                        auditEntries.Add(AuditEntry.Create(
                            entityName,
                            entityId,
                            prop.Metadata.Name,
                            prop.OriginalValue?.ToString(),
                            prop.CurrentValue?.ToString(),
                            currentUserId,
                            entityState));
                    }
                }
            }
            else if (entityState == 3)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    auditEntries.Add(AuditEntry.Create(
                        entityName,
                        entityId,
                        prop.Metadata.Name,
                        prop.OriginalValue?.ToString(),
                        null,
                        currentUserId,
                        entityState));
                }
            }
        }

        if (auditEntries.Count > 0) AddRange(auditEntries);
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
