using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace $safeprojectname$.Application.DTOs.Common;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Timestamp { get; set; }

    public static ApiResponse Success(int statusCode = StatusCodes.Status200OK, string? message = null)
    {
        return new ApiResponse
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message ?? "İşlem başarılı.",
            Errors = null,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static ApiResponse Fail(int statusCode, string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors?.ToList() ?? new List<string> { message },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static ApiResponse<T> Success<T>(T data, int statusCode = StatusCodes.Status200OK, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message ?? "İşlem başarılı.",
            Data = data,
            Errors = null,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static ApiResponse<T> Fail<T>(int statusCode, string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Data = default!,
            Errors = errors?.ToList() ?? new List<string> { message },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static PagedResponse<T> Paged<T>(
        List<T> items,
        int pageNumber,
        int pageSize,
        long totalCount,
        int statusCode = StatusCodes.Status200OK,
        string? message = null)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize));
        return new PagedResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message ?? "İşlem başarılı.",
            Data = items,
            Errors = null,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
