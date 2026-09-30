using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.VerifyTwoFactor;

public record VerifyTwoFactorCommand(string EmailOrUserName, string Code, string? IpAddress = null, string? UserAgent = null) : IRequest<ApiResponse<VerifyTwoFactorResponseDto>>;
