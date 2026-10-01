using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Application.Security;

public class SecurityStampMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public SecurityStampMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext ctx, IUnitOfWork uow)
    {
        if (ctx.User.Identity?.IsAuthenticated != true)
        {
            await _next(ctx);
            return;
        }

        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier) ?? ctx.User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            await _next(ctx);
            return;
        }

        var cacheKey = $"user-stamp:{userId}";
        if (!_cache.TryGetValue<string>(cacheKey, out var cachedStamp))
        {
            var user = await uow.AppUsers.GetByIdAsync(userId);
            cachedStamp = user?.SecurityStamp;

            if (cachedStamp != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(30)
                };
                _cache.Set(cacheKey, cachedStamp, cacheOptions);
            }
        }

        var jwtStamp = ctx.User.FindFirst("security_stamp")?.Value;

        if (cachedStamp != null && jwtStamp != null && cachedStamp != jwtStamp)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";

            var response = ApiResponse.Fail(StatusCodes.Status401Unauthorized, "Security stamp değişmiş, tüm cihazlardan çıkış yapılmıştır.");
            var json = JsonSerializer.Serialize(response);
            await ctx.Response.WriteAsync(json);
            return;
        }

        await _next(ctx);
    }
}
