using MediatR;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.DTOs.UserProfile;

namespace $safeprojectname$.Application.Features.UserProfile.Queries.GetById;

public record GetUserProfileByIdQuery(Guid ProfileId) : IRequest<ApiResponse<UserProfileDto>>;
