using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public sealed class ReceiptResource : Entity<Guid>
{
    public Guid ReceiptDocumentId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }

    private ReceiptResource(Guid id, Guid receiptDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
        : base(id)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        ReceiptDocumentId = receiptDocumentId;
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }

    public static ReceiptResource Create(Guid receiptDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        return new ReceiptResource(Guid.NewGuid(), receiptDocumentId, resourceId, unitOfMeasureId, quantity);
    }

    internal void SetReceiptDocumentId(Guid receiptDocumentId)
    {
        ReceiptDocumentId = receiptDocumentId;
    }
}
