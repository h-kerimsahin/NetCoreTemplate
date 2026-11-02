using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ApiResponse<bool>>;
