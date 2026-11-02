using NetCoreTemplate.Domain.Entities.Seedworks;

namespace NetCoreTemplate.Domain.Entities;

public class AuditEntry : BaseEntity
{
    public string EntityName { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string PropertyName { get; private set; } = null!;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public byte EntityState { get; private set; }

    private AuditEntry() { }

    public static AuditEntry Create(string entityName, Guid entityId, string propertyName, string? oldValue, string? newValue, Guid? changedByUserId, byte entityState)
    {
        return new AuditEntry
        {
            EntityName = entityName,
            EntityId = entityId,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow,
            EntityState = entityState
        };
    }
}
