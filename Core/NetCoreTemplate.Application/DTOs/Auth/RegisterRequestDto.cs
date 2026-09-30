namespace NetCoreTemplate.Application.DTOs.Auth;

public record RegisterRequestDto(string UserName, string Email, string Password, string ConfirmPassword, string? FirstName = null, string? LastName = null);
