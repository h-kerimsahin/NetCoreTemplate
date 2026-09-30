using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace NetCoreTemplate.Infrastructure.Logging;

public static class SerilogConfigurator
{
    public static void Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        const string tableName = "SystemLogs";

        var columnOptions = new ColumnOptions();
        try
        {
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.AdditionalColumns =
            [
                new SqlColumn { ColumnName = "UserId", DataType = System.Data.SqlDbType.UniqueIdentifier, AllowNull = true },
                new SqlColumn { ColumnName = "IpAddress", DataType = System.Data.SqlDbType.NVarChar, DataLength = 64, AllowNull = true },
                new SqlColumn { ColumnName = "RequestPath", DataType = System.Data.SqlDbType.NVarChar, DataLength = 1000, AllowNull = true }
            ];
        }
        catch { /* column options issue, ignore */ }

        loggerConfiguration
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "NetCoreTemplate")
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(LogEventLevel.Information,
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                loggerConfiguration.WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = tableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 50,
                        BatchPeriod = TimeSpan.FromSeconds(2)
                    },
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    columnOptions: columnOptions);
            }
            catch { /* Ignore SQL sink init errors */ }
        }
    }
}
