namespace $safeprojectname$.Application.DTOs.Auth;

public record ResetPasswordRequestDto(string Email, string Token, string NewPassword, string ConfirmNewPassword);
