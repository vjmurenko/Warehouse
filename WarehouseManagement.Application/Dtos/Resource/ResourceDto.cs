namespace WarehouseManagement.Application.Dtos.Resource;

public record ResourceDto(
    Guid Id,
    string Name,
    bool IsActive
);