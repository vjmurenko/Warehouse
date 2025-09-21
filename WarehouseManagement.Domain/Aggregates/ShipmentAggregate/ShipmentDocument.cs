﻿﻿﻿﻿﻿﻿﻿using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public class ShipmentDocument : Entity, IAggregateRoot
{
    public string Number { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsSigned { get; private set; }
    public List<ShipmentResource> ShipmentResources { get; } = new();
    
    // Private constructor for EF Core
    private ShipmentDocument()
    {
    }
    
    // Public constructor with ID generation
    public ShipmentDocument(string number, Guid clientId, DateTime date, bool isSigned = false)
    {
        Id = Guid.NewGuid();
        Number = !string.IsNullOrWhiteSpace(number) ? number.Trim() : throw new ArgumentNullException(nameof(number));
        ClientId = clientId;
        Date = date;
        IsSigned = isSigned;
    }
    
    public void Revoke() 
    {
        var balanceDeltas = GetBalanceDeltas();
        AddDomainEvent(new ShipmentDocumentRevokedEvent(Id, balanceDeltas));
        IsSigned = false;
    }

    public void UpdateNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException(nameof(number));
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

    public void ClearResources()
    {
        ShipmentResources.Clear();
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
        ShipmentResources.Add(resource);
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
        var balanceDeltas = GetBalanceDeltas();
        AddDomainEvent(new ShipmentDocumentSignedEvent(Id, balanceDeltas));
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