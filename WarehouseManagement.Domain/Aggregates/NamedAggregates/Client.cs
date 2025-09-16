using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Client : NamedEntity
{
    public Address Address { get; private set; } = null!; // Will be set by EF Core or constructor

    // Конструктор для EF Core
    private Client() : base()
    {
        // EF Core will set Address property during hydration
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
