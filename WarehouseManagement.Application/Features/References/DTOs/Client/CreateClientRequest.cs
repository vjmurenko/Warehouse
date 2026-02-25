namespace WarehouseManagement.Application.Features.References.DTOs.Client;

public record CreateClientRequest(
    string Name,
    string Address
);