using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.ValueObjects;

public class Quantity : ValueObject
{
    public decimal Value { get; }

    public Quantity(decimal value)
    {
        if (value < 0m)
        {
            throw new ArgumentException("Value can't be less than zero", nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}