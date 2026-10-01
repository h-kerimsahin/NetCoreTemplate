using Microsoft.AspNetCore.Http;

namespace NetCoreTemplate.Api.DTOs;

public class UploadFileRequest
{
    public IFormFile File { get; set; } = default!;
    public string Container { get; set; } = "default";
}