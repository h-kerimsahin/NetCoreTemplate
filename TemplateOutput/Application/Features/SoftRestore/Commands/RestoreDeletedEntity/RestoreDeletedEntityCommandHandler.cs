using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.SoftRestore.Commands.RestoreDeletedEntity;

public class RestoreDeletedEntityCommandHandler : IRequestHandler<RestoreDeletedEntityCommand, ApiResponse<bool>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public RestoreDeletedEntityCommandHandler(IAppDbContext dbContext, IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(RestoreDeletedEntityCommand request, CancellationToken cancellationToken)
    {
        var entityType = Type.GetType(request.EntityTypeName, false, true);
        if (entityType == null)
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "$safeprojectname$.Domain");
            if (domainAssembly != null)
            {
                entityType = domainAssembly.GetType($"$safeprojectname$.Domain.Entities.{request.EntityTypeName}", false, true);
            }
        }

        if (entityType == null)
        {
            throw new BusinessException($"'{request.EntityTypeName}' türü bulunamadı.");
        }

        var setMethod = typeof(IAppDbContext).GetMethod("Set")?.MakeGenericMethod(entityType);
        if (setMethod == null)
        {
            throw new BusinessException("DbSet bulunamadı.");
        }

        var dbSet = setMethod.Invoke(_dbContext, null);
        if (dbSet == null)
        {
            throw new BusinessException("DbSet null geldi.");
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
        var idProperty = System.Linq.Expressions.Expression.Property(parameter, "Id");
        var idConstant = System.Linq.Expressions.Expression.Constant(request.EntityId);
        var idEquals = System.Linq.Expressions.Expression.Equal(idProperty, idConstant);
        var idLambda = System.Linq.Expressions.Expression.Lambda(idEquals, parameter);

        var firstOrDefaultMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "FirstOrDefaultAsync" && m.IsGenericMethod && m.GetParameters().Length == 3)
            .MakeGenericMethod(entityType);

        var entityTask = (Task<object?>)firstOrDefaultMethod.Invoke(null, new object[] { queryable, idLambda, cancellationToken })!;
        var entity = await entityTask;

        if (entity == null)
        {
            throw new NotFoundException(request.EntityTypeName, request.EntityId);
        }

        Guid? currentUserId = null;
        var restoreMethod = entityType.GetMethod("Restore", new[] { typeof(Guid?) });
        if (restoreMethod != null)
        {
            restoreMethod.Invoke(entity, new object?[] { currentUserId });
        }

        var updateGenericMethod = typeof(DbContext).GetMethods()
            .FirstOrDefault(m => m.Name == "Update" && m.IsGenericMethod);
        if (updateGenericMethod != null)
        {
            var updateMethod = updateGenericMethod.MakeGenericMethod(entityType);
            updateMethod.Invoke(_dbContext, new[] { entity });
        }
        else
        {
            var updateMethod = typeof(DbContext).GetMethod("Update", new[] { typeof(object) });
            updateMethod?.Invoke(_dbContext, new[] { entity });
        }

        var auditEntry = AuditEntry.Create(
            entityType.Name,
            request.EntityId,
            "Status",
            ((int)EntityStatus.Deleted).ToString(),
            ((int)EntityStatus.Active).ToString(),
            currentUserId,
            2
        );
        await _unitOfWork.AuditEntries.AddAsync(auditEntry, cancellationToken);

        await _activityLogger.LogAsync(currentUserId, UserActivityType.EntityRestored, $"{entityType.Name} geri yüklendi: {request.EntityId}", entityName: entityType.Name, entityId: request.EntityId, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Varlık başarıyla geri yüklendi.");
    }
}