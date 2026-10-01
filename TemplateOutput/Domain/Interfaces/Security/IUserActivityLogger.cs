using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Interfaces.Security;

public interface IUserActivityLogger
{
    Task LogAsync(Guid? appUserId, UserActivityType activityType, string description, string? entityName = null, Guid? entityId = null, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task LogEntityCreatedAsync(Guid? appUserId, string entityName, Guid entityId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogEntityUpdatedAsync(Guid? appUserId, string entityName, Guid entityId, string changesDescription, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogEntityDeletedAsync(Guid? appUserId, string entityName, Guid entityId, string? ipAddress = null, CancellationToken cancellationToken = default);
}
