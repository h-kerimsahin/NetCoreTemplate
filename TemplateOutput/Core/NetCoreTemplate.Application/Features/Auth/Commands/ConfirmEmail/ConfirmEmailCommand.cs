using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    Guid UserId,
    string? Token = null
) : IRequest<ApiResponse<bool>>;