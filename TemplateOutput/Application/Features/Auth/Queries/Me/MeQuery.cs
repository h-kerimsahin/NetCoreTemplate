using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Queries.Me;

public record MeQuery(Guid UserId) : IRequest<ApiResponse<MeResponseDto>>;
