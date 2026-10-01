namespace $safeprojectname$.Application.DTOs.UserProfile;

public record UpdateUserProfileRequestDto(string FirstName, string LastName, DateOnly? BirthDate, string? PhoneNumber, string? Address, string? City, string? Country, string? AvatarUrl, string? Bio);
