namespace NetCoreTemplate.Application.DTOs.Auth;

public record MeResponseDto(Guid Id, string UserName, string Email, string FirstName, string LastName, string FullName, bool EmailConfirmed, bool TwoFactorEnabled, string? PhoneNumber, string? AvatarUrl, DateTime CreatedDate);
