namespace NetCoreTemplate.Application.DTOs.Auth;

public record LoginResponseDto(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt, bool IsTwoFactorRequired = false);
