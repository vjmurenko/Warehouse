namespace WarehouseManagement.Application.Dtos.Client;

public record CreateClientRequest(
    string Name,
    string Address
);