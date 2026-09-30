namespace NetCoreTemplate.Domain.Interfaces;

public interface IIdempotencyService
{
    Task<bool> ExistsAsync(string key, CancellationToken ct);
    Task<string?> GetCachedResponseAsync(string key, CancellationToken ct);
    Task CacheResponseAsync(string key, int statusCode, string jsonResponse, TimeSpan? slidingExpiration, CancellationToken ct);
}
