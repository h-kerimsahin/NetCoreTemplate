namespace $safeprojectname$.Application.DTOs.Common;

public record NotificationSendRequestDto(
    Guid? UserId,
    string Title,
    string Message,
    int Type,
    string? Data
);
