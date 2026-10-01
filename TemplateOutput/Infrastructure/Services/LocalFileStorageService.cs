using Microsoft.Extensions.Options;
using $safeprojectname$.Domain.Interfaces.Services;

namespace $safeprojectname$.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars", "documents", "temp", "exports"
    };

    private readonly FileStorageSettings _settings;
    private readonly string _webRootPath;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.LocalRootPath);
    }

    public async Task<string> UploadFileAsync(string fileName, string containerName, Stream content, CancellationToken cancellationToken = default)
    {
        fileName = Path.GetFileName(fileName);
        containerName = (containerName ?? string.Empty).Trim();

        if (!AllowedContainers.Contains(containerName))
            throw new ArgumentException($"Invalid container name: {containerName}. Allowed: {string.Join(", ", AllowedContainers)}", nameof(containerName));

        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine("uploads", containerName, now.ToString("yyyy"), now.ToString("MM"), now.ToString("dd"));
        var absoluteDir = Path.Combine(_webRootPath, relativeDir);

        if (!Directory.Exists(absoluteDir))
            Directory.CreateDirectory(absoluteDir);

        var extension = Path.GetExtension(fileName);
        var safeFileName = Guid.NewGuid().ToString("N") + extension;
        var fullPath = Path.Combine(absoluteDir, safeFileName);

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        var relativeUrl = "/" + Path.Combine(relativeDir, safeFileName).Replace(Path.DirectorySeparatorChar, '/');
        return GetPublicUrlAsync(relativeUrl).GetAwaiter().GetResult();
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.FromResult(false);

        var relativePath = ExtractRelativePath(fileUrl);
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);

        var fullPath = Path.Combine(_webRootPath, relativePath.TrimStart('/', '\\'));
        fullPath = Path.GetFullPath(fullPath);

        var normalizedWebRoot = Path.GetFullPath(_webRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!fullPath.StartsWith(normalizedWebRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !fullPath.Equals(normalizedWebRoot, StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        if (!File.Exists(fullPath)) return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.FromResult(false);

        var relativePath = ExtractRelativePath(fileUrl);
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);

        var fullPath = Path.Combine(_webRootPath, relativePath.TrimStart('/', '\\'));
        fullPath = Path.GetFullPath(fullPath);

        var normalizedWebRoot = Path.GetFullPath(_webRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!fullPath.StartsWith(normalizedWebRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !fullPath.Equals(normalizedWebRoot, StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(File.Exists(fullPath));
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

    private static string? ExtractRelativePath(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return null;

        var idx = fileUrl.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) return fileUrl.Substring(idx);

        if (fileUrl.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            return "/" + fileUrl;

        if (fileUrl.StartsWith('/')) return fileUrl;

        return "/" + fileUrl;
    }
}
