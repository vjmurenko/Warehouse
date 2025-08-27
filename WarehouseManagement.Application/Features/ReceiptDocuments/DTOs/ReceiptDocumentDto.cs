namespace WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

public record ReceiptDocumentDto(
    Guid Id,
    string Number,
    DateTime Date,
    List<ReceiptResourceDetailDto> Resources
);

public record ReceiptDocumentSummaryDto(
    Guid Id,
    string Number,
    DateTime Date,
    int ResourceCount
);

public record ReceiptResourceDetailDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitId,
    string UnitName,
    decimal Quantity
);