using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Client : NamedEntity
{
    public Address Address { get; private set; } = null!;
    
    private Client() : base()
    {
        
    }
    
    public Client(string name, string address) : base(name)
    {
        ChangeAddress(address);
    }
    
    public void ChangeAddress(string addressName)
    {
        Address = new Address(addressName);
    }
}
