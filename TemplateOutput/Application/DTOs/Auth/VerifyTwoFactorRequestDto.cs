namespace $safeprojectname$.Application.DTOs.Auth;

public record VerifyTwoFactorRequestDto(string EmailOrUserName, string Code, string? Provider = "Email");
