namespace WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

public record CreateShipmentResourceDto(
    Guid ResourceId,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record ShipmentDocumentDto(
    Guid Id,
    string Number,
    Guid ClientId,
    string ClientName,
    DateTime Date,
    bool IsSigned,
    List<ShipmentResourceDto> Resources
);

public record ShipmentResourceDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureName,
    decimal Quantity
);
