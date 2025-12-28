using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentResource : Entity
{
    public ShipmentResource(Guid shipmentDocumentId, Guid resourceId, Guid unitOfMeasureId, decimal quantity)
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
        return new ShipmentResource(shipmentDocumentId, resourceId, unitOfMeasureId, quantity);
    }

    public Guid ShipmentDocumentId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
}
