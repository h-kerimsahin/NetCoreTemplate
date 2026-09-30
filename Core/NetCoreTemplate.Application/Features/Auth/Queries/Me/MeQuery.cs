using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Queries.Me;

public record MeQuery(Guid UserId) : IRequest<ApiResponse<MeResponseDto>>;
