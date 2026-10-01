namespace $safeprojectname$.Application.DTOs.Common;

public abstract class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? OrderBy { get; set; }
    public bool OrderByDescending { get; set; } = true;
}

public static class PagedRequestExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> source, PagedRequest request)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        return source.Skip(skip).Take(request.PageSize);
    }

    public static IEnumerable<T> ApplyPagination<T>(this IEnumerable<T> source, PagedRequest request)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        return source.Skip(skip).Take(request.PageSize);
    }
}
