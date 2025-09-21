using MediatR;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Events;

// Receipt document events
public record ReceiptDocumentCreatedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceAdjustment> BalanceDeltas
) : INotification;

public record ReceiptDocumentUpdatedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceAdjustment> BalanceDeltas
) : INotification;

public record ReceiptDocumentDeletedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceAdjustment> BalanceDeltas
) : INotification;