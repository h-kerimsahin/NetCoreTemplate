using MediatR;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;
