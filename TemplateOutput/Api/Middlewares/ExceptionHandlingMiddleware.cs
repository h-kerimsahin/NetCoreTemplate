using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using Serilog;

namespace $safeprojectname$.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            Log.Error(ex, "Unhandled exception occurred at {Path} {Method}", context.Request.Path, context.Request.Method);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var logLevel = ex switch
                {
                    NotFoundException or ValidationException or BusinessException => SystemLogLevel.Warning,
                    UnauthorizedException => SystemLogLevel.Warning,
                    _ => SystemLogLevel.Error
                };

                var systemLog = new SystemLog(
                    logLevel: logLevel,
                    message: ex.Message,
                    exceptionType: ex.GetType().FullName,
                    stackTrace: ex.StackTrace,
                    source: ex.Source,
                    requestPath: context.Request.Path,
                    requestMethod: context.Request.Method,
                    userId: userId,
                    ipAddress: ipAddress
                );

                await uow.SystemLogs.AddAsync(systemLog);
                await uow.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                Log.Warning(logEx, "Failed to write SystemLog entry for the exception");
            }

            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

            context.Response.ContentType = "application/json";

            ApiResponse response = ex switch
            {
                NotFoundException ne => ApiResponse.Fail((int)HttpStatusCode.NotFound, ne.Message,
                    new[] { $"Kaynak bulunamadı: {ne.Message}" }),

                ValidationException ve => ApiResponse.Fail((int)HttpStatusCode.BadRequest,
                    "Doğrulama hatası oluştu.", ve.Errors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}"))),

                BusinessException be => ApiResponse.Fail((int)HttpStatusCode.BadRequest, be.Message),

                UnauthorizedException ue => ApiResponse.Fail((int)HttpStatusCode.Unauthorized,
                    string.IsNullOrWhiteSpace(ue.Message) ? "Kimlik doğrulama başarısız." : ue.Message),

                _ => ApiResponse.Fail((int)HttpStatusCode.InternalServerError,
                    env.IsDevelopment() ? ex.ToString() : "Beklenmedik bir sunucu hatası oluştu.")
            };

            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
