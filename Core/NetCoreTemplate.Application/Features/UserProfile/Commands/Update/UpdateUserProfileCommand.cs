using MediatR;
using NetCoreTemplate.Application.DTOs.UserProfile;

namespace NetCoreTemplate.Application.Features.UserProfile.Commands.Update;

public record UpdateUserProfileCommand(Guid AppUserId, string FirstName, string LastName, DateOnly? BirthDate, string? PhoneNumber, string? Address, string? City, string? Country, string? AvatarUrl, string? Bio, string? IpAddress = null) : IRequest<UserProfileDto>;
