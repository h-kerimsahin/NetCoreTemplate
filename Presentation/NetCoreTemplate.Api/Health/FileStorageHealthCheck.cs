using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Api.Health;

public class FileStorageHealthCheck : IHealthCheck
{
    private readonly IFileStorageService _fileStorageService;

    public FileStorageHealthCheck(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _fileStorageService.FileExistsAsync("test.txt", cancellationToken);
            return HealthCheckResult.Healthy("Dosya depolama servisi erişilebilir.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Dosya depolama health check başarısız.", ex);
        }
    }
}
