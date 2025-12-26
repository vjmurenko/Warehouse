using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public sealed class ShipmentDocument : Entity, IAggregateRoot
{
    public string Number { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsSigned { get; private set; }
    
    public IReadOnlyCollection<ShipmentResource> ShipmentResources  => _shipmentResources.AsReadOnly();
    
    private List<ShipmentResource> _shipmentResources = new();
  
    private ShipmentDocument()
    {
    }
    
    // Public constructor with ID generation
    public ShipmentDocument(string number, Guid clientId, DateTime date, bool isSigned = false)
    {
        Id = Guid.NewGuid();
        
        ArgumentException.ThrowIfNullOrWhiteSpace(number, nameof(number));
        Number = number.Trim();
        ClientId = clientId;
        Date = date;
        IsSigned = isSigned;
    }
    
    public void Revoke() 
    {
        AddDomainEvent(new ShipmentDocumentRevokedEvent(Id, GetBalanceDeltas()));
        IsSigned = false;
    }

    public void UpdateNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number.Trim();
    }

    public void UpdateClientId(Guid clientId)
    {
        ClientId = clientId;
    }

    public void UpdateDate(DateTime date)
    {
        Date = date;
    }

    public void SetResources(IEnumerable<BalanceDelta> balanceDeltas)
    {
        ClearResources();
        foreach (var bd in balanceDeltas)
        {
            AddResource(bd.ResourceId, bd.UnitOfMeasureId, bd.Quantity);
        }
        AddDomainEvent(new ShipmentDocumentChangedResourcesEvent(Id, GetBalanceDeltas()));
    }

    public void ClearResources()
    {
        _shipmentResources.Clear();
    }
    
    public void ValidateNotEmpty()
    {
        if (!ShipmentResources.Any())
        {
            throw new InvalidOperationException("Документ отгрузки не может быть пустым");
        }
    }

    public void AddResource(Guid resourceId, Guid unitOfMeasureId, decimal quantity)
    {
        var resource = new ShipmentResource(resourceId, unitOfMeasureId, quantity)
        {
            ShipmentDocumentId = Id
        };
        _shipmentResources.Add(resource);
    }
    
    private void Validate()
    {
        if (!ShipmentResources.Any())
        {
            throw new InvalidOperationException("Документ отгрузки не может быть пустым");
        }
    }

    public void Sign()
    {
        Validate();
        AddDomainEvent(new ShipmentDocumentSignedEvent(Id, GetBalanceDeltas()));
        IsSigned = true;
    }
    
    private IReadOnlyCollection<BalanceDelta> GetBalanceDeltas()
    {
        return ShipmentResources
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .Select(g => new BalanceDelta(g.Key.ResourceId, g.Key.UnitOfMeasureId, g.Sum(r => r.Quantity.Value)))
            .ToList();
    }
}