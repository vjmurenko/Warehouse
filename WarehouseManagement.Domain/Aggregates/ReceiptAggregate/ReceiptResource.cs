using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public sealed class ReceiptResource : Entity
{
    private ReceiptResource() { }

    public ReceiptResource(Guid receiptDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        ReceiptDocumentId = receiptDocumentId;
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }

    public Guid ReceiptDocumentId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
}
