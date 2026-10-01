using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.DTOs.UserProfile;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Application.Features.UserProfile.Queries.GetById;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, ApiResponse<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserProfileByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.AppUserProfiles.GetByIdAsync(request.ProfileId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUserProfile), request.ProfileId);
        var dto = _mapper.Map<UserProfileDto>(profile);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Profil bilgileri başarıyla getirildi.");
    }
}
