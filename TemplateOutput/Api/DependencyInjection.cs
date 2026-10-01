using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Filters;
using $safeprojectname$.Api.ApiVersioning;
using $safeprojectname$.Api.Endpoints;
using $safeprojectname$.Api.Health;
using $safeprojectname$.Api.Hubs;
using $safeprojectname$.Api.Idempotency;
using $safeprojectname$.Api.Middlewares;
using $safeprojectname$.Api.OpenTelemetry;
using $safeprojectname$.Application;
using $safeprojectname$.Infrastructure;
using $safeprojectname$.Infrastructure.Idempotency;
using $safeprojectname$.Infrastructure.Logging;
using Scalar.AspNetCore;
using Serilog;

namespace $safeprojectname$.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var env = builder.Environment;

        builder.Host.UseSerilog((ctx, cfg) => SerilogConfigurator.Configure(cfg, ctx.Configuration));

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(config);
        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<IdempotencySettings>(config.GetSection("Idempotency"));

        builder.Services.AddRateLimiter(options =>
        {
            options.RegisterPolicy();
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: $"global-{ip}",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 500,
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 15,
                        QueueLimit = 0
                    });
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("SecureCorsPolicy", policy =>
            {
                if (env.IsDevelopment() && CorsSettings.AllowAllInDevelopment)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders(CorsSettings.ExposedHeaders);
                }
                else
                {
                    policy.WithOrigins(CorsSettings.AllowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders(CorsSettings.ExposedHeaders);

                    if (CorsSettings.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                }
            });
        });

        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "db" })
            .AddCheck<SmtpHealthCheck>("smtp", tags: new[] { "ready", "smtp" })
            .AddCheck<FileStorageHealthCheck>("file-storage", tags: new[] { "ready", "storage" })
            .AddCheck<HangfireHealthCheck>("hangfire", tags: new[] { "ready", "hangfire" });

        builder.Services.AddApiVersioningAndOData();
        builder.Services.AddSignalR();
        builder.Services.AddOTelServices(config);

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Version = "v1.0",
                Title = "$safeprojectname$ API v1",
                Description = "$safeprojectname$ Clean Architecture v1 API Endpoints (Default)"
            });
            c.SwaggerDoc("v2", new Microsoft.OpenApi.OpenApiInfo
            {
                Version = "v2.0",
                Title = "$safeprojectname$ API v2",
                Description = "$safeprojectname$ Clean Architecture v2 API Endpoints (Test/Preview)"
            });

            c.EnableAnnotations();
            c.OperationFilter<SecurityRequirementsOperationFilter>();
            c.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Name = "Authorization",
                Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        builder.Services.AddEndpointsApiExplorer();

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
                ClockSkew = TimeSpan.Zero,
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var env = app.Environment;
        var config = app.Configuration;

        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        app.UseCors("SecureCorsPolicy");
        app.UseRateLimiter();

        app.UseMiddleware<$safeprojectname$.Application.Security.SecurityStampMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "$safeprojectname$ API v1.0");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "$safeprojectname$ API v2.0");
            });

            app.MapSwagger("/{documentName}/openapi.json")
               .WithDisplayName("Swagger JSON OpenAPI format");
            app.MapSwagger("/swagger/{documentName}/swagger.json")
               .WithDisplayName("Swagger JSON swagger.json format");

            app.MapScalarApiReference("scalar", c =>
            {
                c.OpenApiRoutePattern = "/swagger/v1/swagger.json";
                c.Title = "$safeprojectname$ API v1.0 Documentation";
            });
            app.MapScalarApiReference("scalar/v2", c =>
            {
                c.OpenApiRoutePattern = "/swagger/v2/swagger.json";
                c.Title = "$safeprojectname$ API v2.0 Documentation";
            });
        }

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            AllowCachingResponses = false
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains("ready"),
            AllowCachingResponses = false,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.ToString("g"),
                    entries = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => new
                        {
                            status = entry.Value.Status.ToString(),
                            description = entry.Value.Description,
                            duration = entry.Value.Duration.ToString("g"),
                            exception = entry.Value.Exception?.Message,
                            data = entry.Value.Data
                        }
                    )
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        });

        app.MapPrometheusScrapingEndpoint("/metrics");

        app.MapHub<NotificationHub>("/hubs/notification");

        app.MapControllers();
        app.MapEndpoints();

        try
        {
            var storageTest = app.Services.GetService<JobStorage>();
            if (storageTest != null)
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireAdminAuthorizationFilter() },
                    IgnoreAntiforgeryToken = true,
                    StatsPollingInterval = 2000,
                    DashboardTitle = config["Hangfire:DashboardTitle"] ?? "$safeprojectname$ Hangfire Dashboard"
                });
                HangfireJobScheduler.InitializeRecurringJobs(app.Services);
                Log.Information("Hangfire dashboard and recurring jobs enabled via DI storage.");
            }
            else
            {
                Log.Warning("Hangfire JobStorage not registered in DI — skipping Hangfire Dashboard.");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Hangfire storage unavailable — skipping Hangfire Dashboard and recurring jobs. Fix SQL/LocalDB to enable Hangfire.");
        }

        return app;
    }
}

public static class HangfireJobScheduler
{
    public static void InitializeRecurringJobs(IServiceProvider? services = null)
    {
        try
        {
            IRecurringJobManager manager = services?.GetService<IRecurringJobManager>()
                ?? new RecurringJobManager();

            manager.AddOrUpdate<$safeprojectname$.Infrastructure.Jobs.NightlyCleanupJob>(
                "nightly-cleanup",
                job => job.Execute(CancellationToken.None),
                Cron.Daily(2, 0),
                new Hangfire.RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        }
        catch
        {
        }
    }
}
