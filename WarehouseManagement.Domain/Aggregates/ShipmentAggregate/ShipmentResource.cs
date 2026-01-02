using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentResource : Entity<Guid>
{
    public Guid ShipmentDocumentId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    
    private ShipmentResource(Guid id, Guid shipmentDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
        : base(id)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        ShipmentDocumentId = shipmentDocumentId;
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }

    public static ShipmentResource Create(Guid shipmentDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        return new ShipmentResource(Guid.NewGuid(), shipmentDocumentId, resourceId, unitOfMeasureId, quantity);
    }
    
    internal void SetShipmentDocumentId(Guid shipmentDocumentId)
    {
        ShipmentDocumentId = shipmentDocumentId;
    }
}
