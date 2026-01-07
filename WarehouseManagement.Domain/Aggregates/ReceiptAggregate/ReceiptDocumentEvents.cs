using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Events;

public record ReceiptDocumentCreatedEvent(Guid DocumentId) : Event;

public record ReceiptDocumentUpdatedEvent(Guid DocumentId) : Event;

public record ReceiptDocumentDeletedEvent(Guid DocumentId) : Event;
