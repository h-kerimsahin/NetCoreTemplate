using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand(Guid UserId, string Type = "Email") : IRequest<ApiResponse<bool>>;
