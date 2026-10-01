namespace $safeprojectname$.Domain.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string fileName, string containerName, Stream content, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<string> GetPublicUrlAsync(string fileUrl);
}
