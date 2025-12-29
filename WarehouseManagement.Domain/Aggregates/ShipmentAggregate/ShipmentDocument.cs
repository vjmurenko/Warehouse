using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentDocument : AggregateRoot<Guid>
{
    private readonly List<ShipmentResource> _shipmentResources = [];

    public string Number { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsSigned { get; private set; }

    public IReadOnlyCollection<ShipmentResource> ShipmentResources => _shipmentResources.AsReadOnly();

    // EF Core constructor
    private ShipmentDocument(Guid id, string number, Guid clientId, DateTime date, bool isSigned) : base(id)
    {
        Number = number;
        ClientId = clientId;
        Date = date;
        IsSigned = isSigned;
    }
    
    private ShipmentDocument(Guid id, string number, Guid clientId, DateTime date, IEnumerable<ShipmentResource> resources, bool isSigned = false) 
        : this(id, number, clientId, date, isSigned)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number, nameof(number));
        Number = number.Trim();
        foreach (var resource in resources)
        {
            resource.SetShipmentDocumentId(id);
            _shipmentResources.Add(resource);
        }
    }

    public static ShipmentDocument Create(string number, Guid clientId, DateTime date, IEnumerable<ShipmentResource> resources)
    {
        return new ShipmentDocument(Guid.NewGuid(), number, clientId, date, resources);
    }

    public void Revoke()
    {
        Raise(new ShipmentDocumentRevokedEvent(Id));
        IsSigned = false;
    }

    public void UpdateNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number.Trim();
    }

    public void UpdateClientId(Guid clientId) => ClientId = clientId;

    public void UpdateDate(DateTime date) => Date = date;

    public void SetResources(IEnumerable<ShipmentResource> resources)
    {
        _shipmentResources.Clear();
        foreach (var resource in resources.Where(c => c.Quantity > 0))
        {
            resource.SetShipmentDocumentId(Id);
            _shipmentResources.Add(resource);
        }
        Raise(new ShipmentDocumentChangedResourcesEvent(Id));
    }

    public void ClearResources() => _shipmentResources.Clear();

    public void ValidateNotEmpty()
    {
        if (_shipmentResources.Count == 0)
            throw new InvalidOperationException("Документ отгрузки не может быть пустым");
    }

    public void Sign()
    {
        ValidateNotEmpty();
        Raise(new ShipmentDocumentSignedEvent(Id));
        IsSigned = true;
    }
}
