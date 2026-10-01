using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.SoftRestore.Commands.RestoreDeletedEntity;

public record RestoreDeletedEntityCommand(
    string EntityTypeName,
    Guid EntityId
) : IRequest<ApiResponse<bool>>;