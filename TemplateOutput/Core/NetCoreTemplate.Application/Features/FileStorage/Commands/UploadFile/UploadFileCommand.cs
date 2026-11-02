using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.FileStorage.Commands.UploadFile;

public record UploadFileCommand(
    string FileName,
    string ContainerName,
    byte[] Content,
    long Size,
    string ContentType,
    Guid UserId
) : IRequest<ApiResponse<FileUploadResultDto>>;