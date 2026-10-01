using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace NetCoreTemplate.Api.Middlewares;

public static class RateLimitingPolicy
{
    public const string AuthFixedWindowPolicy = "AuthFixedWindow";
    public const string PerUserSlidingWindowPolicy = "PerUserSlidingWindow";
    public const string GlobalIpSlidingWindowPolicy = "GlobalIpSlidingWindow";

    public static RateLimiterOptions RegisterPolicy(this RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy<string>(AuthFixedWindowPolicy, httpContext =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/api/v1/auth", StringComparison.OrdinalIgnoreCase) ||
                httpContext.Request.Path.StartsWithSegments("/api/v2/auth", StringComparison.OrdinalIgnoreCase))
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"auth-{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    });
            }
            return RateLimitPartition.GetNoLimiter("no-limit");
        });

        options.AddPolicy<string>(PerUserSlidingWindowPolicy, httpContext =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && httpContext.User.Identity?.IsAuthenticated == true)
            {
                var key = $"{userId}-{httpContext.Request.Method}-{httpContext.Request.Path}";
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: key,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 150,
                        Window = TimeSpan.FromSeconds(60),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
            }
            return RateLimitPartition.GetNoLimiter("no-limit");
        });

        options.AddPolicy<string>(GlobalIpSlidingWindowPolicy, httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: $"global-{ip}",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 500,
                    Window = TimeSpan.FromMinutes(15),
                    SegmentsPerWindow = 15,
                    QueueLimit = 0
                });
        });

        return options;
    }
}
