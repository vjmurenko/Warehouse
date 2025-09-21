using MediatR;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Events;

// Receipt document events
public record ReceiptDocumentCreatedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceDelta> BalanceDeltas
) : INotification;

public record ReceiptDocumentUpdatedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceDelta> BalanceDeltas
) : INotification;

public record ReceiptDocumentDeletedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceDelta> BalanceDeltas
) : INotification;