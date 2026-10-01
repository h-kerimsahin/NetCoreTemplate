using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Infrastructure.Persistence;

namespace $safeprojectname$.Api.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return HealthCheckResult.Healthy("Veritabanı bağlantısı başarılı.");
            }
            return HealthCheckResult.Degraded("Veritabanına bağlanılamıyor.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Veritabanı health check başarısız.", ex);
        }
    }
}
