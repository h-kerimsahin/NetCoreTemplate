using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreTemplate.Application.DTOs.Settings;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Repositories;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;
using NetCoreTemplate.Infrastructure.Idempotency;
using NetCoreTemplate.Infrastructure.Jobs;
using NetCoreTemplate.Infrastructure.Logging;
using NetCoreTemplate.Infrastructure.Persistence;
using NetCoreTemplate.Infrastructure.Persistence.Repositories;
using NetCoreTemplate.Infrastructure.Security;
using NetCoreTemplate.Infrastructure.Services;

namespace NetCoreTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IAppUserProfileRepository, AppUserProfileRepository>();
        services.AddScoped<IAppUserRefreshTokenRepository, AppUserRefreshTokenRepository>();
        services.AddScoped<IAppUserActivitedRepository, AppUserActivitedRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IAppRoleRepository, AppRoleRepository>();
        services.AddScoped<IAppUserRoleRepository, AppUserRoleRepository>();
        services.AddScoped<IAppPermissionRepository, AppPermissionRepository>();
        services.AddScoped<IAppRolePermissionRepository, AppRolePermissionRepository>();
        services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();
        services.AddScoped<IAppNotificationRepository, AppNotificationRepository>();
        services.AddScoped<IBackgroundJobLogRepository, BackgroundJobLogRepository>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<IdempotencySettings>(configuration.GetSection("Idempotency"));
        services.Configure<LockoutSettings>(configuration.GetSection("LockoutSettings"));
        services.Configure<PasswordSettings>(configuration.GetSection("PasswordSettings"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

        services.AddMemoryCache();
        services.AddScoped<IIdempotencyService, MemoryIdempotencyService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserActivityLogger, UserActivityLogger>();
        services.AddScoped<IEmailService, EmailService>();

        if (string.Equals(configuration["FileStorage:Provider"], "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        services.AddScoped<INotificationService, DatabaseNotificationService>();

        ConfigureHangfire(services, connectionString);

        services.AddScoped<EmailSenderJob>();
        services.AddScoped<NightlyCleanupJob>();

        return services;
    }

    private static void ConfigureHangfire(IServiceCollection services, string connectionString)
    {
        Action<IGlobalConfiguration> useStorage = cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
               {
                   PrepareSchemaIfNecessary = true,
                   SchemaName = "Hangfire",
                   CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                   SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                   QueuePollInterval = TimeSpan.Zero,
                   UseRecommendedIsolationLevel = true,
                   DisableGlobalLocks = true
               });
        };

        try
        {
            using var test = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            test.Open();
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Hangfire cannot connect to SQL storage. Falling back to In-Memory storage for this run.");
            useStorage = cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage(new MemoryStorageOptions
                {
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5)
                });
        }

        try
        {
            services.AddHangfire(useStorage);
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 5;
                options.Queues = new[] { "default", "email", "notification", "recurring" };
            });
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Hangfire server registration failed — Hangfire will be disabled for this run.");
        }
    }
}
