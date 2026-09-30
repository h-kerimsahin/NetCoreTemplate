using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId, string? IpAddress = null) : IRequest<LogoutResponse>;
