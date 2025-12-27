using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;

namespace WarehouseManagement.Domain.Aggregates;

public sealed class StockMovement : Entity
{
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid DocumentId { get; private set; }
    public MovementType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    public StockMovement(Guid resourceId, Guid unitId, decimal quantity, Guid documentId, MovementType type)
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        UnitOfMeasureId = unitId;
        Quantity = quantity;
        DocumentId = documentId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public static StockMovement CreateReversal(StockMovement original)
    {
        return new StockMovement(
            original.ResourceId,
            original.UnitOfMeasureId,
            -original.Quantity,
            original.DocumentId,
            original.Type);
    }
}
