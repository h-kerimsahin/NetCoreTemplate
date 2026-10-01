using MediatR;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.DTOs.UserProfile;

namespace $safeprojectname$.Application.Features.UserProfile.Commands.Update;

public record UpdateUserProfileCommand(Guid AppUserId, string FirstName, string LastName, DateOnly? BirthDate, string? PhoneNumber, string? Address, string? City, string? Country, string? AvatarUrl, string? Bio, string? IpAddress = null) : IRequest<ApiResponse<UserProfileDto>>;
