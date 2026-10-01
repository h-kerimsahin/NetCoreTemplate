using Microsoft.Extensions.Options;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Infrastructure.Idempotency;
using System.Security.Claims;
using System.Text.Json;

namespace NetCoreTemplate.Api.Idempotency;

public class IdempotencyEndpointFilter : IEndpointFilter
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly IdempotencySettings _settings;

    public IdempotencyEndpointFilter(IIdempotencyService idempotencyService, IOptions<IdempotencySettings> settings)
    {
        _idempotencyService = idempotencyService;
        _settings = settings.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var idempotencyKey = request.Headers["X-Idempotency-Key"].ToString();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return await next(context);
        }

        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var httpMethod = request.Method;
        var path = request.Path.ToString();
        var compositeKey = $"{userId}|{httpMethod}|{path}|{idempotencyKey}";

        if (await _idempotencyService.ExistsAsync(compositeKey, httpContext.RequestAborted))
        {
            var cachedEntryJson = await _idempotencyService.GetCachedResponseAsync(compositeKey, httpContext.RequestAborted);
            if (!string.IsNullOrWhiteSpace(cachedEntryJson))
            {
                var cachedEntry = JsonSerializer.Deserialize<CachedResponseEntry>(cachedEntryJson);
                if (cachedEntry != null)
                {
                    httpContext.Response.StatusCode = cachedEntry.StatusCode;
                    httpContext.Response.ContentType = "application/json";
                    return cachedEntry.JsonResponse;
                }
            }
        }

        var originalBodyStream = httpContext.Response.Body;
        using var memoryStream = new MemoryStream();
        httpContext.Response.Body = memoryStream;

        try
        {
            var result = await next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Position = 0;

            await memoryStream.CopyToAsync(originalBodyStream, httpContext.RequestAborted);

            var statusCode = httpContext.Response.StatusCode;
            if (statusCode is >= 200 and <= 299)
            {
                var slidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes);
                await _idempotencyService.CacheResponseAsync(compositeKey, statusCode, responseBody, slidingExpiration, httpContext.RequestAborted);
            }

            return result;
        }
        finally
        {
            httpContext.Response.Body = originalBodyStream;
        }
    }

    private class CachedResponseEntry
    {
        public int StatusCode { get; set; }
        public string JsonResponse { get; set; } = string.Empty;
    }
}
