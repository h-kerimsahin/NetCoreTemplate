using $safeprojectname$.Domain.Entities.Seedworks;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Entities;

public class AppUserActivited : BaseEntity
{
    public Guid? AppUserId { get; private set; }
    public UserActivityType ActivityType { get; private set; }
    public string Description { get; private set; } = null!;
    public string? EntityName { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public virtual AppUser? AppUser { get; private set; }

    private AppUserActivited() { }

    public AppUserActivited(Guid? appUserId, UserActivityType activityType, string description, string? entityName = null, Guid? entityId = null, string? ipAddress = null, string? userAgent = null)
    {
        AppUserId = appUserId;
        ActivityType = activityType;
        Description = description;
        EntityName = entityName;
        EntityId = entityId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
