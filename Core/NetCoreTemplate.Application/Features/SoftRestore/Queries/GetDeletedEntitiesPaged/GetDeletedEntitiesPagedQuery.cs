using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.SoftRestore.Queries.GetDeletedEntitiesPaged;

public record GetDeletedEntitiesPagedQuery(
    string EntityTypeName,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResponse<object>>;

public class GetDeletedEntitiesPagedQueryHandler : IRequestHandler<GetDeletedEntitiesPagedQuery, PagedResponse<object>>
{
    private readonly IAppDbContext _dbContext;

    public GetDeletedEntitiesPagedQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<object>> Handle(GetDeletedEntitiesPagedQuery request, CancellationToken cancellationToken)
    {
        var entityType = Type.GetType(request.EntityTypeName, false, true);
        if (entityType == null)
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "NetCoreTemplate.Domain");
            if (domainAssembly != null)
            {
                entityType = domainAssembly.GetType($"NetCoreTemplate.Domain.Entities.{request.EntityTypeName}", false, true);
            }
        }

        if (entityType == null)
        {
            return ApiResponse.Paged<object>(new List<object>(), request.PageNumber, request.PageSize, 0, 200, $"'{request.EntityTypeName}' türü bulunamadı.");
        }

        var pagedRequest = new PagedRequestImpl { PageNumber = request.PageNumber, PageSize = request.PageSize };

        var setMethod = typeof(IAppDbContext).GetMethod("Set")?.MakeGenericMethod(entityType);
        if (setMethod == null)
        {
            return ApiResponse.Paged<object>(new List<object>(), request.PageNumber, request.PageSize, 0, 200, "DbSet bulunamadı.");
        }

        var dbSet = setMethod.Invoke(_dbContext, null);
        if (dbSet == null)
        {
            return ApiResponse.Paged<object>(new List<object>(), request.PageNumber, request.PageSize, 0, 200, "DbSet null geldi.");
        }

        var asQueryableMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "AsQueryable" && m.IsGenericMethod)
            .MakeGenericMethod(entityType);
        var queryable = (IQueryable)asQueryableMethod.Invoke(null, new[] { dbSet })!;

        var ignoreFiltersMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "IgnoreQueryFilters" && m.IsGenericMethod)
            .MakeGenericMethod(entityType);
        queryable = (IQueryable)ignoreFiltersMethod.Invoke(null, new object[] { queryable })!;

        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, "Status");
        var constant = System.Linq.Expressions.Expression.Constant(EntityStatus.Deleted);
        var equals = System.Linq.Expressions.Expression.Equal(property, constant);
        var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

        var whereMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(entityType);
        queryable = (IQueryable)whereMethod.Invoke(null, new object[] { queryable, lambda })!;

        var countMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "CountAsync" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(CancellationToken))
            .MakeGenericMethod(entityType);
        var totalCountTask = (Task<int>)countMethod.Invoke(null, new object[] { queryable, cancellationToken })!;
        var totalCount = await totalCountTask;

        var skip = (pagedRequest.PageNumber - 1) * pagedRequest.PageSize;
        var skipMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Skip" && m.IsGenericMethod)
            .MakeGenericMethod(entityType);
        queryable = (IQueryable)skipMethod.Invoke(null, new object[] { queryable, skip })!;

        var takeMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Take" && m.IsGenericMethod)
            .MakeGenericMethod(entityType);
        queryable = (IQueryable)takeMethod.Invoke(null, new object[] { queryable, pagedRequest.PageSize })!;

        var toListMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "ToListAsync" && m.IsGenericMethod)
            .MakeGenericMethod(entityType);
        var toListTask = (System.Collections.IEnumerable)toListMethod.Invoke(null, new object[] { queryable, cancellationToken })!;
        var items = ((System.Collections.IEnumerable)toListTask).Cast<object>().ToList();

        return ApiResponse.Paged<object>(items, request.PageNumber, request.PageSize, totalCount, 200, "Silinmiş varlıklar başarıyla getirildi.");
    }
}

public class PagedRequestImpl : PagedRequest { }