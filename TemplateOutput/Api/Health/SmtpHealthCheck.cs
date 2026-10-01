using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using $safeprojectname$.Infrastructure.Services;

namespace $safeprojectname$.Api.Health;

public class SmtpHealthCheck : IHealthCheck
{
    private readonly EmailSettings _emailSettings;

    public SmtpHealthCheck(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                return HealthCheckResult.Degraded("SMTP sunucusuna bağlantı zaman aşımına uğradı.");
            }

            if (tcpClient.Connected)
            {
                return HealthCheckResult.Healthy("SMTP sunucusuna bağlantı başarılı.");
            }

            return HealthCheckResult.Unhealthy("SMTP sunucusuna bağlanılamadı.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SMTP health check başarısız.", ex);
        }
    }
}
