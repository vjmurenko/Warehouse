namespace WarehouseManagement.Application.Features.References.DTOs.Resource;

public record ResourceDto(
    Guid Id,
    string Name,
    bool IsActive
);