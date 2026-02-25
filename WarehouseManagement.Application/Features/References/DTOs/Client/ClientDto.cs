namespace WarehouseManagement.Application.Features.References.DTOs.Client;

public record ClientDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive
);