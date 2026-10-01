namespace $safeprojectname$.Application.DTOs.Common;

public record RoleAssignRequestDto(
    Guid UserId,
    List<Guid> RoleIds
);
