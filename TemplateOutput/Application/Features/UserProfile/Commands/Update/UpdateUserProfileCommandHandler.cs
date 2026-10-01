using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.DTOs.UserProfile;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.UserProfile.Commands.Update;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserActivityLogger _activityLogger;

    public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.AppUserProfiles.GetByAppUserIdAsync(request.AppUserId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUserProfile), request.AppUserId);

        profile.Update(request.FirstName, request.LastName, request.BirthDate, request.PhoneNumber, request.Address, request.City, request.Country, request.AvatarUrl, request.Bio);
        _unitOfWork.AppUserProfiles.Update(profile);

        await _activityLogger.LogAsync(request.AppUserId, UserActivityType.ProfileUpdated, "User profile updated", nameof(Domain.Entities.AppUserProfile), profile.Id, request.IpAddress, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserProfileDto>(profile);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Profil güncelleme başarılı.");
    }
}
