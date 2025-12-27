using MediatR;

namespace WarehouseManagement.Domain.Events;

public record ReceiptDocumentCreatedEvent(Guid DocumentId) : INotification;

public record ReceiptDocumentUpdatedEvent(Guid DocumentId) : INotification;

public record ReceiptDocumentDeletedEvent(Guid DocumentId) : INotification;
