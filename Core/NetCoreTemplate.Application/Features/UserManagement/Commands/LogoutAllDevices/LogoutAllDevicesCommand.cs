using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.UserManagement.Commands.LogoutAllDevices;

public record LogoutAllDevicesCommand(Guid UserId) : IRequest<ApiResponse<bool>>;