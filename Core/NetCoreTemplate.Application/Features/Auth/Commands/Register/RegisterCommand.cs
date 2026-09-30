using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string UserName, string Email, string Password, string ConfirmPassword, string? FirstName, string? LastName, string? IpAddress = null) : IRequest<RegisterResponseDto>;
