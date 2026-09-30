using MediatR;
using NetCoreTemplate.Application.DTOs.UserProfile;

namespace NetCoreTemplate.Application.Features.UserProfile.Queries.GetById;

public record GetUserProfileByIdQuery(Guid ProfileId) : IRequest<UserProfileDto>;
