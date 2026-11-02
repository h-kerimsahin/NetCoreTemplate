namespace NetCoreTemplate.Application.DTOs.Auth;

public record RefreshTokenResponseDto(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);
