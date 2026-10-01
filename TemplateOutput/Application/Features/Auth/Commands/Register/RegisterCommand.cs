using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string UserName, string Email, string Password, string ConfirmPassword, string? FirstName, string? LastName, string? IpAddress = null) : IRequest<ApiResponse<RegisterResponseDto>>;
