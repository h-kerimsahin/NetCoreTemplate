using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;
using $safeprojectname$.Domain.Interfaces.Services;

namespace $safeprojectname$.Application.Features.FileStorage.Commands.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserActivityLogger _activityLogger;

    public DeleteFileCommandHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.DeleteFileAsync(request.FileUrl, cancellationToken);

        await _activityLogger.LogAsync(null, UserActivityType.FileDeleted, $"Dosya silindi: {request.FileUrl}", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(result, StatusCodes.Status200OK, "Dosya başarıyla silindi.");
    }
}