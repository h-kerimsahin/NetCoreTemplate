using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Application.Features.AuditEntries.Queries.GetAuditByUserIdPaged;

public record GetAuditByUserIdPagedQuery(
    Guid ChangedByUserId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResponse<AuditEntryDto>>;

public class GetAuditByUserIdPagedQueryHandler : IRequestHandler<GetAuditByUserIdPagedQuery, PagedResponse<AuditEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAuditByUserIdPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<AuditEntryDto>> Handle(GetAuditByUserIdPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var query = _unitOfWork.AuditEntries.GetAll()
            .Where(a => a.ChangedByUserId == request.ChangedByUserId)
            .OrderByDescending(a => a.ChangedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPagination(pagedRequest)
            .ProjectTo<AuditEntryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount, 200, "Kullanıcı denetim kayıtları başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }