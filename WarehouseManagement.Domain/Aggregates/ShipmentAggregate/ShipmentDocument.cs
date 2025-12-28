using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentDocument : Entity, IAggregateRoot
{
    private readonly List<ShipmentResource> _shipmentResources = new();
    public string Number { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsSigned { get; private set; }

    public IReadOnlyCollection<ShipmentResource> ShipmentResources => _shipmentResources.AsReadOnly();

    public ShipmentDocument(string number, Guid clientId, DateTime date, bool isSigned = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number, nameof(number));
        Number = number.Trim();
        ClientId = clientId;
        Date = date;
        IsSigned = isSigned;
    }

    public void Revoke()
    {
        AddDomainEvent(new ShipmentDocumentRevokedEvent(Id));
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
        
        AddResources(resources.Where(c => c.Quantity >0));
        
        AddDomainEvent(new ShipmentDocumentChangedResourcesEvent(Id));
    }

    public void ClearResources() => _shipmentResources.Clear();

    public void ValidateNotEmpty()
    {
        if (_shipmentResources.Count == 0)
            throw new InvalidOperationException("Документ отгрузки не может быть пустым");
    }

    public void AddResources(IEnumerable<ShipmentResource> resources)
    {
        _shipmentResources.AddRange(resources);
    }

    public void Sign()
    {
        ValidateNotEmpty();
        AddDomainEvent(new ShipmentDocumentSignedEvent(Id));
        IsSigned = true;
    }

    public IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> GetResourceItems()
    {
        return _shipmentResources.Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity));
    }
}
