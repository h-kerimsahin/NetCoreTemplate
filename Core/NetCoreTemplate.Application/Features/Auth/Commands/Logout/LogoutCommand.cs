using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId, string? IpAddress = null) : IRequest<ApiResponse<LogoutResponse>>;
