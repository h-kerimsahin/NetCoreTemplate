using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Application.Features.Auth.Queries.Me;

public class MeQueryHandler : IRequestHandler<MeQuery, ApiResponse<MeResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<MeResponseDto>> Handle(MeQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdWithProfileAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.AppUser), request.UserId);
        var dto = _mapper.Map<MeResponseDto>(user);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Kullanıcı bilgileri başarıyla getirildi.");
    }
}
