using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;

namespace NetCoreTemplate.Application.Features.Auth.Queries.Me;

public record MeQuery(Guid UserId) : IRequest<MeResponseDto>;
