namespace $safeprojectname$.Application.DTOs.Common;

public record AuditEntryDto(
    Guid Id,
    string EntityName,
    string EntityId,
    string PropertyName,
    string? OldValue,
    string? NewValue,
    Guid? ChangedByUserId,
    DateTime ChangedAt,
    string EntityState
);
