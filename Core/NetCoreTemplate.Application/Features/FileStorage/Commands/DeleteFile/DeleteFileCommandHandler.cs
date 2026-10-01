using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Application.Features.FileStorage.Commands.DeleteFile;

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