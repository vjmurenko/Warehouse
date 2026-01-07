using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Events;

public record ShipmentDocumentSignedEvent(Guid DocumentId) : Event;

public record ShipmentDocumentRevokedEvent(Guid DocumentId) : Event;

public record ShipmentDocumentChangedResourcesEvent(Guid DocumentId) : Event;
