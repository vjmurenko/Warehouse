using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptResource : Entity
{
    private ReceiptResource()
    {
        Quantity = new Quantity(0);
    }
    
    public ReceiptResource(Guid receiptDocumentId, Guid resourceId, Guid unitOfMeasureId, Quantity quantity)
    {
        ReceiptDocumentId = receiptDocumentId;
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }

    public Guid ReceiptDocumentId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public Quantity Quantity { get; set; }
}
