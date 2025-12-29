using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public sealed class Client : NamedEntity
{
    public Address Address { get; private set; } = null!;

    // EF Core uses this constructor + ComplexProperty for Address
    private Client(Guid id, string name, bool isActive) : base(id, name, isActive)
    {
    }

    public static Client Create(string name, Address address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(address);
        
        var client = new Client(Guid.NewGuid(), name, true);
        client.Address = address;
        return client;
    }
    
    public void ChangeAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}
