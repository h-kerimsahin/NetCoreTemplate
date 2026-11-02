using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.AuditEntries.Queries.GetAuditByEntityIdPaged;

public record GetAuditByEntityIdPagedQuery(
    string EntityName,
    Guid EntityId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResponse<AuditEntryDto>>;

public class GetAuditByEntityIdPagedQueryHandler : IRequestHandler<GetAuditByEntityIdPagedQuery, PagedResponse<AuditEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAuditByEntityIdPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<AuditEntryDto>> Handle(GetAuditByEntityIdPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var query = _unitOfWork.AuditEntries.GetAll()
            .Where(a => a.EntityName == request.EntityName && a.EntityId == request.EntityId)
            .OrderByDescending(a => a.ChangedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPagination(pagedRequest)
            .ProjectTo<AuditEntryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount, 200, "Denetim kayıtları başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }