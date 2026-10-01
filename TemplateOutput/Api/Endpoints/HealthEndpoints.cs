using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Api.Endpoints;

public class HealthEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/health");

        group.MapGet("live", () =>
        {
            var response = ApiResponse.Success(StatusCodes.Status200OK, "Liveness Healthy");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("HealthLiveness").AllowAnonymous();

        group.MapGet("ready", async (HealthCheckService healthCheckService) =>
        {
            var result = await healthCheckService.CheckHealthAsync();
            if (result.Status == HealthStatus.Healthy)
            {
                var response = ApiResponse.Success(new { Status = result.Status.ToString(), result.TotalDuration }, StatusCodes.Status200OK, "Readiness Healthy");
                return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
            }
            else
            {
                var errors = result.Entries.Select(e => $"{e.Key}: {e.Value.Status}").ToList();
                var response = ApiResponse.Fail(StatusCodes.Status503ServiceUnavailable, "Readiness Unhealthy", errors);
                return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
            }
        }).WithName("HealthReadiness").AllowAnonymous();

        group.MapGet("detailed", async (HealthCheckService healthCheckService) =>
        {
            var result = await healthCheckService.CheckHealthAsync();
            var details = result.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                e.Value.Description,
                e.Value.Duration,
                Data = e.Value.Data,
                Exception = e.Value.Exception?.Message
            }).ToList();

            var payload = new
            {
                OverallStatus = result.Status.ToString(),
                TotalDuration = result.TotalDuration,
                Checks = details
            };

            var response = result.Status == HealthStatus.Healthy
                ? ApiResponse.Success(payload, StatusCodes.Status200OK, "Detailed Health Check")
                : ApiResponse.Success(payload, StatusCodes.Status200OK, "Detailed Health Check (Bazı servisler sağlıksız)");

            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("HealthDetailed").AllowAnonymous();

        group.MapGet("metrics", () =>
        {
            var response = ApiResponse.Success(StatusCodes.Status200OK, "Prometheus endpoint: /metrics");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("HealthMetrics").AllowAnonymous();
    }
}
