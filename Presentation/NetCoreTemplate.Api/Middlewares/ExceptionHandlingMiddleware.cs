using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using Serilog;

namespace NetCoreTemplate.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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

            var (statusCode, response) = ex switch
            {
                NotFoundException ne => (HttpStatusCode.NotFound, (object)new { error = "Not Found", message = ne.Message }),
                ValidationException ve => (HttpStatusCode.BadRequest, (object)new { error = "Validation Error", message = ve.Message, errors = ve.Errors }),
                BusinessException be => (HttpStatusCode.BadRequest, (object)new { error = "Business Rule Violation", message = be.Message }),
                UnauthorizedException ue => (HttpStatusCode.Unauthorized, (object)new { error = "Unauthorized", message = ue.Message }),
                _ => (HttpStatusCode.InternalServerError, (object)new { error = "Internal Server Error", message = env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred" })
            };

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
