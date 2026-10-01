using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.VerifyTwoFactor;

public record VerifyTwoFactorCommand(string EmailOrUserName, string Code, string? IpAddress = null, string? UserAgent = null) : IRequest<ApiResponse<VerifyTwoFactorResponseDto>>;
