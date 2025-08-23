namespace WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

public record CreateReceiptResourceDto(
    Guid ResourceId,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record ReceiptDocumentDto(
    Guid Id,
    string Number,
    DateTime Date,
    List<ReceiptResourceDto> Resources
);

public record ReceiptResourceDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureName,
    decimal Quantity
);
