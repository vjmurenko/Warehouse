namespace WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

public record ShipmentResourceDto(
    Guid ResourceId,
    Guid UnitId,
    decimal Quantity
);