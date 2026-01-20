using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Aggregates;

public sealed class StockMovement : Entity<Guid>
{
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid DocumentId { get; private set; }
    public MovementType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private StockMovement(Guid id, Guid resourceId, Guid unitOfMeasureId, decimal quantity, Guid documentId, MovementType type)
        : base(id)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
        DocumentId = documentId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public static StockMovement Create(Guid resourceId, Guid unitId, decimal quantity, Guid documentId, MovementType type)
    {
        return new StockMovement(Guid.NewGuid(), resourceId, unitId, quantity, documentId, type);
    }
    
    public static StockMovement CreateReversal(StockMovement original)
    {
        return Create(
            original.ResourceId,
            original.UnitOfMeasureId,
            -original.Quantity,
            original.DocumentId,
            original.Type);
    }
}
