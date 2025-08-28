using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates;

public class Balance : Entity, IAggregateRoot
{
    public Guid ResourceId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public Quantity Quantity { get; set; }
    
    public Balance(Guid resourceId, Guid unitOfMeasureId, Quantity quantity)
    {
        ResourceId = resourceId;
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
    }
    
    private Balance()
    {
        Quantity = new Quantity(0);
    }

    public void Increase(Quantity amount)
    {
        if (amount == null)
        {
            throw new ArgumentNullException(nameof(amount));
        }
        Quantity = new Quantity(Quantity.Value + amount.Value);
    }

    public void Decrease(Quantity amount)
    {
        if (amount == null)
        {
            throw new ArgumentNullException(nameof(amount));
        }

        if (Quantity.Value < amount.Value)
        {
            throw new InvalidOperationException($"Not enought money to decrease, balance is {Quantity.Value}");
        }
        Quantity = new Quantity(Quantity.Value - amount.Value);
    }
}