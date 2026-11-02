namespace NetCoreTemplate.Application.DTOs.Common;

public record FileUploadResultDto(
    string FileUrl,
    string FileName,
    long SizeBytes,
    string ContentType
);
