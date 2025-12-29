using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Events;

public record ReceiptDocumentCreatedEvent(Guid DocumentId) : Event;

public record ReceiptDocumentUpdatedEvent(Guid DocumentId) : Event;

public record ReceiptDocumentDeletedEvent(Guid DocumentId) : Event;
