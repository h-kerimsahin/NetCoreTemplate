namespace NetCoreTemplate.Application.DTOs.Auth;

public record VerifyTwoFactorResponseDto(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);
