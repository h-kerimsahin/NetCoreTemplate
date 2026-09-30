using AutoMapper;
using MediatR;
using NetCoreTemplate.Application.DTOs.UserProfile;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.UserProfile.Commands.Update;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
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

    public async Task<UserProfileDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.AppUserProfiles.GetByAppUserIdAsync(request.AppUserId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUserProfile), request.AppUserId);

        profile.Update(request.FirstName, request.LastName, request.BirthDate, request.PhoneNumber, request.Address, request.City, request.Country, request.AvatarUrl, request.Bio);
        _unitOfWork.AppUserProfiles.Update(profile);

        await _activityLogger.LogAsync(request.AppUserId, UserActivityType.ProfileUpdated, "User profile updated", nameof(Domain.Entities.AppUserProfile), profile.Id, request.IpAddress, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserProfileDto>(profile);
    }
}
