using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars", "documents", "temp", "exports"
    };

    private readonly FileStorageSettings _settings;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
        _blobServiceClient = new BlobServiceClient(_settings.AzureBlobConnectionString);
    }

    public async Task<string> UploadFileAsync(string fileName, string containerName, Stream content, CancellationToken cancellationToken = default)
    {
        fileName = Path.GetFileName(fileName);
        containerName = (containerName ?? string.Empty).Trim();

        if (!AllowedContainers.Contains(containerName))
            throw new ArgumentException($"Invalid container name: {containerName}. Allowed: {string.Join(", ", AllowedContainers)}", nameof(containerName));

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToLowerInvariant());
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;
        var extension = Path.GetExtension(fileName);
        var blobName = $"{now:yyyy}/{now:MM}/{now:dd}/{Guid.NewGuid():N}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(extension)
            }
        };

        content.Position = 0;
        await blobClient.UploadAsync(content, options, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return false;

        var parsed = ParseBlobUrl(fileUrl);
        if (parsed == null) return false;

        var (containerName, blobName) = parsed.Value;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var result = await blobClient.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots, null, cancellationToken);
        return result.Value;
    }

    public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return false;

        var parsed = ParseBlobUrl(fileUrl);
        if (parsed == null) return false;

        var (containerName, blobName) = parsed.Value;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var result = await blobClient.ExistsAsync(cancellationToken);
        return result.Value;
    }

    public Task<string> GetPublicUrlAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.FromResult(string.Empty);

        if (fileUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            fileUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(fileUrl);

        var baseUrl = _settings.AppBaseUrl?.TrimEnd('/') ?? string.Empty;
        var path = fileUrl.StartsWith('/') ? fileUrl : "/" + fileUrl;
        return Task.FromResult(baseUrl + path);
    }

    private (string ContainerName, string BlobName)? ParseBlobUrl(string fileUrl)
    {
        try
        {
            if (fileUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                fileUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments.Skip(1).Select(s => s.TrimEnd('/')).Where(s => !string.IsNullOrEmpty(s)).ToList();
                if (segments.Count < 2) return null;

                var containerName = segments[0];
                var blobName = string.Join("/", segments.Skip(1));
                return (containerName, blobName);
            }

            var idx = fileUrl.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
            var path = idx >= 0 ? fileUrl.Substring(idx + "/uploads/".Length) : fileUrl.TrimStart('/');
            var parts = path.Split('/', 2);
            if (parts.Length < 2) return null;

            return (parts[0], parts[1]);
        }
        catch
        {
            return null;
        }
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            ".7z" => "application/x-7z-compressed",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }
}
