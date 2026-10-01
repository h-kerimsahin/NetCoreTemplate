using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.Notifications.Queries.GetMyNotificationsPaged;

public record GetMyNotificationsPagedQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10,
    bool OnlyUnread = false
) : IRequest<PagedResponse<NotificationDto>>;

public class GetMyNotificationsPagedQueryHandler : IRequestHandler<GetMyNotificationsPagedQuery, PagedResponse<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyNotificationsPagedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<NotificationDto>> Handle(GetMyNotificationsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var query = _unitOfWork.AppNotifications.GetAll()
            .Where(n => (n.UserId == null || n.UserId == request.UserId));

        if (request.OnlyUnread)
        {
            query = query.Where(n => !n.IsRead);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPagination(pagedRequest)
            .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount, 200, "Bildirimler başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }