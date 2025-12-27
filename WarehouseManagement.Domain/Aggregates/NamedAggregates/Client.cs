using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public sealed class Client : NamedEntity
{
    public Address Address { get; private set; } = null!;
    
    private Client() : base()
    {
        
    }
    
    public Client(string name, Address address) : base(name)
    {
        ChangeAddress(address);
    }
    
    public void ChangeAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}
