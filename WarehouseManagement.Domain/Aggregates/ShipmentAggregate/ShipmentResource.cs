using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public class ShipmentResource(Guid resourceId, Guid unitOfMeasureId, Quantity quantity) : Entity
{
    public Guid ResourceId { get; set; } = resourceId;
    public Guid UnitOfMeasureId { get; set; } = unitOfMeasureId;
    public Quantity Quantity { get; set; } = quantity;
}