using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Infrastructure.Idempotency;

public class MemoryIdempotencyService : IIdempotencyService
{
    private readonly IMemoryCache _cache;

    public MemoryIdempotencyService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        var exists = _cache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }

    public Task<string?> GetCachedResponseAsync(string key, CancellationToken ct)
    {
        if (_cache.TryGetValue(key, out string? cached))
        {
            return Task.FromResult(cached);
        }
        return Task.FromResult<string?>(null);
    }

    public Task CacheResponseAsync(string key, int statusCode, string jsonResponse, TimeSpan? slidingExpiration, CancellationToken ct)
    {
        var entry = new IdempotencyCacheEntry
        {
            StatusCode = statusCode,
            JsonResponse = jsonResponse
        };
        var entryJson = JsonSerializer.Serialize(entry);

        var options = new MemoryCacheEntryOptions();
        if (slidingExpiration.HasValue)
        {
            options.SlidingExpiration = slidingExpiration.Value;
        }

        _cache.Set(key, entryJson, options);
        return Task.CompletedTask;
    }

    private class IdempotencyCacheEntry
    {
        public int StatusCode { get; set; }
        public string JsonResponse { get; set; } = string.Empty;
    }
}
