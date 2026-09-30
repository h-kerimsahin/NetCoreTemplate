using NetCoreTemplate.Api;
using Serilog;

try
{
    Log.Information("Starting NetCoreTemplate API");
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureServices();
    var app = builder.Build();
    app.ConfigurePipeline();
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
