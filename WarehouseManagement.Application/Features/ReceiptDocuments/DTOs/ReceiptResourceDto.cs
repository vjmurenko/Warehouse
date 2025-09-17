namespace WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

public record ReceiptResourceDto(
    Guid ResourceId,
    Guid UnitId,
    decimal Quantity
);

