namespace $safeprojectname$.Application.DTOs.Auth;

public record RefreshTokenResponseDto(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);
