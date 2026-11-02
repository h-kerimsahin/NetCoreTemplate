using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.UserManagement.Queries.GetAllUsersPaged;

public record GetAllUsersPagedQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null
) : IRequest<PagedResponse<UserDto>>;

public class GetAllUsersPagedQueryHandler : IRequestHandler<GetAllUsersPagedQuery, PagedResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsersPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<UserDto>> Handle(GetAllUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var query = _unitOfWork.AppUsers.GetAll();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u =>
                u.UserName.Contains(request.Search) ||
                u.Email.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPagination(pagedRequest)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount, 200, "Kullanıcılar başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }