namespace NetCoreTemplate.Application.DTOs.Common;

public record PermissionGrantDto(
    Guid RoleId,
    List<Guid> PermissionIds
);
