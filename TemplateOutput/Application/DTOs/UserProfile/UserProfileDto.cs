namespace $safeprojectname$.Application.DTOs.UserProfile;

public record UserProfileDto(Guid Id, Guid AppUserId, string FirstName, string LastName, string FullName, DateOnly? BirthDate, string? PhoneNumber, string? Address, string? City, string? Country, string? AvatarUrl, string? Bio);
