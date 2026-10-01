using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace $safeprojectname$.Api.Health;

public class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var statistics = monitoringApi.GetStatistics();
            var serversCount = statistics.Servers;

            if (serversCount > 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Hangfire sunucuları aktif çalışıyor."));
            }

            return Task.FromResult(HealthCheckResult.Degraded("Hangfire sunucusu bulunamadı."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire health check başarısız.", ex));
        }
    }
}
