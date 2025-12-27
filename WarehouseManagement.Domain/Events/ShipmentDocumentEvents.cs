using MediatR;

namespace WarehouseManagement.Domain.Events;

public record ShipmentDocumentSignedEvent(Guid DocumentId) : INotification;

public record ShipmentDocumentRevokedEvent(Guid DocumentId) : INotification;

public record ShipmentDocumentChangedResourcesEvent(Guid DocumentId) : INotification;
