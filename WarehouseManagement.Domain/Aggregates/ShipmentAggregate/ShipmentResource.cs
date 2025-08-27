using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public class ShipmentResource : Entity
{
    // Конструктор для EF Core
    private ShipmentResource()
    {
        Quantity = new Quantity(0);
    }
    
    public ShipmentResource(Guid resourceId, Guid unitOfMeasureId, Quantity quantity)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
    }
    
    public ShipmentResource(Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = new Quantity(quantity);
    }

    public Guid ShipmentDocumentId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public Quantity Quantity { get; set; }
}
