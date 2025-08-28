namespace WarehouseManagement.Application.Dtos.Client;

public record UpdateClientRequest(
    string Name,
    string Address
);