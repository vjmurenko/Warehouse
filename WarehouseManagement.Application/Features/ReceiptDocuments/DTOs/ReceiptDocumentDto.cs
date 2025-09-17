namespace WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

public record ReceiptDocumentDto(
    Guid Id,
    string Number,
    DateTime Date,
    List<ReceiptResourceDetailDto> Resources
)
{
    public int ResourceCount => Resources?.Count ?? 0;
};

public record ReceiptResourceDetailDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitId,
    string UnitName,
    decimal Quantity
);