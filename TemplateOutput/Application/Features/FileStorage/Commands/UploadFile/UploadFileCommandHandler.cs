using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;
using $safeprojectname$.Domain.Interfaces.Services;

namespace $safeprojectname$.Application.Features.FileStorage.Commands.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ApiResponse<FileUploadResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserActivityLogger _activityLogger;

    public UploadFileCommandHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<FileUploadResultDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(request.Content);
        var fileUrl = await _fileStorageService.UploadFileAsync(request.FileName, request.ContainerName, stream, cancellationToken);

        await _activityLogger.LogAsync(request.UserId, UserActivityType.FileUploaded, $"Dosya yüklendi: {request.FileName} ({request.ContainerName})", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new FileUploadResultDto(fileUrl, request.FileName, request.Size, request.ContentType);
        return ApiResponse.Success(result, StatusCodes.Status201Created, "Dosya başarıyla yüklendi.");
    }
}