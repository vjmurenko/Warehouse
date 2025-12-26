using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Name { get; private set; } = string.Empty;
    
    public Address(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
    
    private Address()
    {
        Name = string.Empty; 
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}