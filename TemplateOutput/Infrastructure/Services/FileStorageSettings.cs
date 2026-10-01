namespace $safeprojectname$.Infrastructure.Services;

public class FileStorageSettings
{
    public string Provider { get; set; } = "LocalFile";
    public string AppBaseUrl { get; set; } = "https://localhost:7000";
    public string LocalRootPath { get; set; } = "wwwroot";
    public string AzureBlobConnectionString { get; set; } = "";
    public string AzureBlobContainerName { get; set; } = "uploads";
}
