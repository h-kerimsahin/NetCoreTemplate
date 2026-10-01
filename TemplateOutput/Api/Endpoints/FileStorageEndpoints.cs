using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using $safeprojectname$.Api.DTOs;
using $safeprojectname$.Api.Idempotency;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Interfaces.Services;

namespace $safeprojectname$.Api.Endpoints;

public class FileStorageEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/file-storage")
            .WithTags("FileStorage")
            .RequireAuthorization();

        group.MapPost("upload", async (
            [FromForm] UploadFileRequest request,
            ClaimsPrincipal user,
            [FromServices] IFileStorageService storageService,
            CancellationToken ct) =>
        {
            using var stream = request.File.OpenReadStream();

            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
            var result = await storageService.UploadFileAsync(fileName, request.Container, stream, ct);

            var resp = ApiResponse.Success(new { Url = result, FileName = fileName, OriginalName = request.File.FileName, Size = request.File.Length }, StatusCodes.Status201Created, "Dosya yüklendi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("UploadFile")
          .DisableAntiforgery()
          .Accepts<UploadFileRequest>("multipart/form-data")
          .AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapDelete("delete", async ([FromQuery] string filePath, [FromServices] IFileStorageService storageService) =>
        {
            var deleted = await storageService.DeleteFileAsync(filePath);
            if (!deleted)
            {
                var fail = ApiResponse.Fail(StatusCodes.Status404NotFound, "Dosya bulunamadı.");
                return Results.Json(fail, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: fail.StatusCode);
            }
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Dosya silindi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("DeleteFile");
    }
}
