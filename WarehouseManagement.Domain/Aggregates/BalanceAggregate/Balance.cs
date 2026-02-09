using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Aggregates.BalanceAggregate;

public sealed class Balance : AggregateRoot<Guid>
{
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }

    private Balance(Guid id, Guid resourceId, Guid unitOfMeasureId, decimal quantity) : base(id)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }

    public static Balance Create(Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        return new Balance(Guid.NewGuid(), resourceId, unitOfMeasureId, quantity);
    }

    public void AddQuantity(decimal amount)
    {
        Quantity += amount;
    }

    public void SubtractQuantity(decimal amount)
    {
        Quantity -= amount;
    }

    public void SetQuantity(decimal quantity)
    {
        Quantity = quantity;
    }
}
