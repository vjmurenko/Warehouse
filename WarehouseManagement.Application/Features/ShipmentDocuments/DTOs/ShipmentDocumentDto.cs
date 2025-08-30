namespace WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

public record ShipmentDocumentDto(
    Guid Id,
    string Number,
    Guid ClientId,
    string ClientName,
    DateTime Date,
    bool IsSigned,
    List<ShipmentResourceDetailDto> Resources
)
{
    public int ResourceCount => Resources?.Count ?? 0;
};

public record ShipmentResourceDetailDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitId,
    string UnitName,
    decimal Quantity
);