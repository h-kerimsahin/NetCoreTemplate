using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Repositories;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;
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

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IAppUserProfileRepository, AppUserProfileRepository>();
        services.AddScoped<IAppUserRefreshTokenRepository, AppUserRefreshTokenRepository>();
        services.AddScoped<IAppUserActivitedRepository, AppUserActivitedRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserActivityLogger, UserActivityLogger>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
