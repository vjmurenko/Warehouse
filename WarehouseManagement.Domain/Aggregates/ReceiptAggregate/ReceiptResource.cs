using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptResource(Guid receiptDocumentId, Guid resourceId, Guid unitOfMeasureId, Quantity quantity) : Entity
{
    public Guid ReceiptDocumentId { get; set; } = receiptDocumentId;
    public Guid ResourceId { get; set; } = resourceId;
    public Guid UnitOfMeasureId { get; set; } = unitOfMeasureId;
    public Quantity Quantity { get; set; } = quantity;
}