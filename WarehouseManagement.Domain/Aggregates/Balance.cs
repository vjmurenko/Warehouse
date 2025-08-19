using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates;

public class Balance(Guid resourceId, Guid unitOfMeasureId, Quantity quantity) : Entity, IAggregateRoot
{
    public Guid ResourceId { get; set; } = resourceId;
    public Guid UnitOfMeasureId { get; set; } = unitOfMeasureId;
    public Quantity Quantity { get; set; } = quantity ?? throw new  ArgumentNullException(nameof(quantity));

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
            throw new InvalidOperationException($"Not enought money to decrease, balance is {Quantity}");
        }
        Quantity = new Quantity(Quantity.Value - amount.Value);
    }
}