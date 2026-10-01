namespace $safeprojectname$.Application.DTOs.Common;

public record RoleDto(
    Guid Id,
    string Name,
    string? NormalizedName,
    string? Description,
    List<PermissionDto>? Permissions
);
