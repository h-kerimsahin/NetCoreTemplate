using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Application.Features.Roles.Queries.GetAllRolesPaged;

public record GetAllRolesPagedQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<RoleDto>>;

public class GetAllRolesPagedQueryHandler : IRequestHandler<GetAllRolesPagedQuery, PagedResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllRolesPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<RoleDto>> Handle(GetAllRolesPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var query = _unitOfWork.AppRoles.GetAll();
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPagination(pagedRequest)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount, 200, "Roller başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }