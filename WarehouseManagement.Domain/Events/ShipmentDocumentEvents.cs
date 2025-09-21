using MediatR;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Events;

// Shipment document events
public record ShipmentDocumentSignedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceDelta> BalanceDeltas
) : INotification;

public record ShipmentDocumentRevokedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceDelta> BalanceDeltas
) : INotification;