namespace $safeprojectname$.Application.DTOs.Common;

public record PermissionDto(
    Guid Id,
    string Name,
    string Code,
    string GroupName,
    string? Description
);
