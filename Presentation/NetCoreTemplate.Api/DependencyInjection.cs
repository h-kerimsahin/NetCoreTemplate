using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NetCoreTemplate.Api.Endpoints;
using NetCoreTemplate.Api.Idempotency;
using NetCoreTemplate.Application;
using NetCoreTemplate.Infrastructure;
using NetCoreTemplate.Infrastructure.Idempotency;
using NetCoreTemplate.Infrastructure.Logging;
using Scalar.AspNetCore;
using Serilog;

namespace NetCoreTemplate.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Host.UseSerilog((ctx, cfg) => SerilogConfigurator.Configure(cfg, ctx.Configuration));

        builder.Services.AddControllers();
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(config);
        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<IdempotencySettings>(config.GetSection("Idempotency"));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings.SecretKey is not configured");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetCoreTemplate v1"));
        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseMiddleware<Middlewares.ExceptionHandlingMiddleware>();
        app.UseCors("AllowAll");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapEndpoints();

        return app;
    }
}
