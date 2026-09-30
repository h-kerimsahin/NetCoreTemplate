using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUserName, string Password, bool RememberMe, string? IpAddress = null, string? UserAgent = null) : IRequest<ApiResponse<LoginResponseDto>>;
