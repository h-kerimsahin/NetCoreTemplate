using MediatR;

namespace NetCoreTemplate.Application.Features.Auth.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand(Guid UserId, string Type = "Email") : IRequest<bool>;
