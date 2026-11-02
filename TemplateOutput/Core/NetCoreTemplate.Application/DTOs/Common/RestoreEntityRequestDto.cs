namespace NetCoreTemplate.Application.DTOs.Common;

public record RestoreEntityRequestDto(
    string EntityTypeName,
    Guid EntityId
);
