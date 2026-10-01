namespace $safeprojectname$.Application.DTOs.Common;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    int Type,
    bool IsRead,
    DateTime CreatedAt,
    string? Data
);
