namespace $safeprojectname$.Application.DTOs.Auth;

public record LoginRequestDto(string EmailOrUserName, string Password, bool RememberMe = false);
