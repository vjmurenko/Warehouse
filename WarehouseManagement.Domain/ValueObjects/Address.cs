﻿﻿using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Name { get; private set; } = string.Empty;
    
    public Address(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : throw new ArgumentNullException(nameof(name));
    }
    
    private Address()
    {
        Name = string.Empty; // EF Core will set the actual value later
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}