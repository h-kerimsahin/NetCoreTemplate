using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Infrastructure.Security;

public class UserActivityLogger : IUserActivityLogger
{
    private readonly IUnitOfWork _unitOfWork;

    public UserActivityLogger(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task LogAsync(Guid? appUserId, UserActivityType activityType, string description, string? entityName = null, Guid? entityId = null, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var activity = new Domain.Entities.AppUserActivited(appUserId, activityType, description, entityName, entityId, ipAddress, userAgent);
        await _unitOfWork.AppUserActivities.AddAsync(activity, cancellationToken);
    }

    public async Task LogEntityCreatedAsync(Guid? appUserId, string entityName, Guid entityId, string? ipAddress = null, CancellationToken cancellationToken = default) =>
        await LogAsync(appUserId, UserActivityType.EntityCreated, $"Entity {entityName} created", entityName, entityId, ipAddress, cancellationToken: cancellationToken);

    public async Task LogEntityUpdatedAsync(Guid? appUserId, string entityName, Guid entityId, string changesDescription, string? ipAddress = null, CancellationToken cancellationToken = default) =>
        await LogAsync(appUserId, UserActivityType.EntityUpdated, changesDescription, entityName, entityId, ipAddress, cancellationToken: cancellationToken);

    public async Task LogEntityDeletedAsync(Guid? appUserId, string entityName, Guid entityId, string? ipAddress = null, CancellationToken cancellationToken = default) =>
        await LogAsync(appUserId, UserActivityType.EntityDeleted, $"Entity {entityName} deleted", entityName, entityId, ipAddress, cancellationToken: cancellationToken);
}
