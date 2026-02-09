using System.ComponentModel.DataAnnotations.Schema;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.ValueObjects;

[ComplexType]
public sealed class Address : ValueObject
{
    public string Name { get; private set; }
    
    public Address(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}