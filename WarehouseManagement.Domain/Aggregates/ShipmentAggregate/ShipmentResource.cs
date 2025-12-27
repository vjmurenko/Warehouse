using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentResource : Entity
{
    private ShipmentResource() { }

    public ShipmentResource(Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }
    
    public Guid ShipmentDocumentId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal Quantity { get; set; }
}
