using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NetCoreTemplate.Api.OpenTelemetry;

public static class OTelConfiguration
{
    public static IServiceCollection AddOTelServices(this IServiceCollection services, IConfiguration config)
    {
        var otlpEndpoint = config["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";
        var disableConsoleExporter = config.GetValue<bool>("OpenTelemetry:DisableConsoleExporter");
        var disableOtlpExporter = config.GetValue<bool>("OpenTelemetry:DisableOtlpExporter");

        services.AddOpenTelemetry()
            .ConfigureResource(b => b.AddService("NetCoreTemplate.Api")
                                      .AddTelemetrySdk()
                                      .AddEnvironmentVariableDetector())
            .WithTracing(b =>
            {
                b.SetSampler<AlwaysOnSampler>();
                b.AddAspNetCoreInstrumentation(o =>
                {
                    o.Filter = ctx => ctx.Request.Path != "/health/live" &&
                                      ctx.Request.Path != "/health/ready" &&
                                      ctx.Request.Path != "/metrics";
                    o.RecordException = true;
                });
                b.AddEntityFrameworkCoreInstrumentation();
                b.AddHttpClientInstrumentation(o => o.RecordException = true);
                if (!disableOtlpExporter) b.AddOtlpExporter(o => { o.Endpoint = new Uri(otlpEndpoint); });
                if (!disableConsoleExporter) b.AddConsoleExporter();
            })
            .WithMetrics(b =>
            {
                b.AddAspNetCoreInstrumentation();
                b.AddHttpClientInstrumentation();
                if (!disableOtlpExporter) b.AddOtlpExporter(o => { o.Endpoint = new Uri(otlpEndpoint); });
                if (!disableConsoleExporter) b.AddConsoleExporter();
                b.AddPrometheusExporter(o =>
                {
                    o.ScrapeResponseCacheDurationMilliseconds = 300;
                });
            });

        return services;
    }
}
