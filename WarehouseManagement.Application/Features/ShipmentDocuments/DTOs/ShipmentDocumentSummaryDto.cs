namespace WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

public record ShipmentDocumentSummaryDto(
    Guid Id,
    string Number,
    Guid ClientId,
    string ClientName,
    DateTime Date,
    bool IsSigned,
    int ResourceCount
);