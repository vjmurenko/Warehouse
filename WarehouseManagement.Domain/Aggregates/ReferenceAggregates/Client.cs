using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public sealed class Client : Reference
{
    public Address Address { get; private set; } = null!;

    // EF Core constructor
    private Client(Guid id, string name) : base(id, name)
    {
    }

    public static Client Create(string name, Address address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(address);
        
        var client = new Client(Guid.NewGuid(), name)
        {
            Address = address
        };
        return client;
    }
    
    public void ChangeAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}
