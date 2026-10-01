using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.FileStorage.Commands.DeleteFile;

public record DeleteFileCommand(string FileUrl) : IRequest<ApiResponse<bool>>;