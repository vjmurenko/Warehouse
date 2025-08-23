using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Client : NamedEntity
{
    public Address Address { get; private set; }

    // Конструктор для EF Core
    private Client() : base(string.Empty)
    {
        Address = new Address(string.Empty);
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
