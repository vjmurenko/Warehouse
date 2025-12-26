using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates;

public sealed class Balance : Entity, IAggregateRoot
{
    public Guid ResourceId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public Quantity Quantity { get; private set; }
    
    public Balance(Guid resourceId, Guid unitOfMeasureId, Quantity quantity)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
    }
    
    private Balance()
    {
        Quantity = new Quantity(0);
    }

    public void Increase(Quantity amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Quantity = new Quantity(Quantity.Value + amount.Value);
    }

    public void Decrease(Quantity amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (Quantity.Value < amount.Value)
        {
            throw new InvalidOperationException($"Not enought money to decrease, balance is {Quantity.Value}");
        }
        Quantity = new Quantity(Quantity.Value - amount.Value);
    }
}