using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public class ShipmentDocument(string number, Guid clientId, DateTime date, bool isSigned = false)
    : Entity, IAggregateRoot
{
    public string Number { get;} = !string.IsNullOrWhiteSpace(number) ? number.Trim() : throw new ArgumentNullException(nameof(number));
    public Guid ClientId { get;  } = clientId;
    public DateTime Date { get; private set; } = date;
    public bool IsSigned { get; private set; } = isSigned;
    public List<ShipmentResource> ShipmentResources { get; } = new();
    
    public void Revoke() => IsSigned = false;


    public void AddResource(ShipmentResource resource)
    {
        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }
        ShipmentResources.Add(resource);
    }

    public void RemoveResource(ShipmentResource resource)
    {
        ShipmentResources.Remove(resource);
    }
    private void Validate()
    {
        var a = ShipmentResources;
        if (!ShipmentResources.Any())
        {
            throw new ArgumentNullException("Shipment resources can't be empty");
        }
    }

    public void Sign()
    {
        Validate();
        IsSigned = true;
    }
}