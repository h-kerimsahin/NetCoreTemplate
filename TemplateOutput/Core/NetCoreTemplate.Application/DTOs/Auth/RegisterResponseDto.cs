namespace NetCoreTemplate.Application.DTOs.Auth;

public record RegisterResponseDto(Guid UserId, string UserName, string Email, bool RequiresEmailConfirmation = true);
