using MediatR;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Events;

// Shipment document events
public record ShipmentDocumentSignedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceAdjustment> BalanceDeltas
) : INotification;

public record ShipmentDocumentRevokedEvent(
    Guid DocumentId,
    IReadOnlyCollection<BalanceAdjustment> BalanceDeltas
) : INotification;