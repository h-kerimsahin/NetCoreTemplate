using $safeprojectname$.Api;
using $safeprojectname$.Infrastructure.Persistence.SeedData;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting $safeprojectname$ API (Bootstrap logger)");
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureServices();
    var app = builder.Build();
    AppDbInitializer.InitializeDatabaseAsync(app.Services).GetAwaiter().GetResult();
    app.ConfigurePipeline();
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly — BOOTSTRAP EXCEPTION");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
