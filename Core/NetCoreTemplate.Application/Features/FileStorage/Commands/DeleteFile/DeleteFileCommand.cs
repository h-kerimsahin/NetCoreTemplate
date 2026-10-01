using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.FileStorage.Commands.DeleteFile;

public record DeleteFileCommand(string FileUrl) : IRequest<ApiResponse<bool>>;