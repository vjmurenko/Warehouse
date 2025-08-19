using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.ValueObjects;

public class Address(string name) : ValueObject
{
    public string Name { get; } = !string.IsNullOrWhiteSpace(name) ? name.Trim() : throw new ArgumentNullException(nameof(name));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}