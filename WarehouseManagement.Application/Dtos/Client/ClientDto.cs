namespace WarehouseManagement.Application.Dtos.Client;

public record ClientDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive
);