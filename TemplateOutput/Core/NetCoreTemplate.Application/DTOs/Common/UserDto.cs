namespace NetCoreTemplate.Application.DTOs.Common;

public record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    string? PhoneNumber,
    string? AvatarUrl,
    DateTime CreatedDate,
    bool IsLockedOut,
    int AccessFailedCount
);