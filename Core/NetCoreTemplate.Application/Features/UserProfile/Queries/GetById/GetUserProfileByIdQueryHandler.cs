using AutoMapper;
using MediatR;
using NetCoreTemplate.Application.DTOs.UserProfile;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.UserProfile.Queries.GetById;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserProfileByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.AppUserProfiles.GetByIdAsync(request.ProfileId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUserProfile), request.ProfileId);
        return _mapper.Map<UserProfileDto>(profile);
    }
}
