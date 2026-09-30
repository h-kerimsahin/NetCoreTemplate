using AutoMapper;
using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.Auth.Queries.Me;

public class MeQueryHandler : IRequestHandler<MeQuery, MeResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MeResponseDto> Handle(MeQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdWithProfileAsync(request.UserId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUser), request.UserId);
        return _mapper.Map<MeResponseDto>(user);
    }
}
