﻿﻿﻿﻿﻿using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocument : Entity, IAggregateRoot
{
    private readonly List<ReceiptResource> _receiptResources = new();
    public string Number { get; private set; }
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();
    
    private ReceiptDocument() { }

    public ReceiptDocument(string number, DateTime date)
    {
        Id = Guid.NewGuid(); 
        Number = number ?? throw new ArgumentNullException(nameof(number));
        Date = date;
        
        // Add domain event for receipt creation - will be handled after resources are added
    }
    
    public void AddResource(Guid resourceId, Guid unitId, decimal quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше 0");

        var resource = new ReceiptResource(Id, resourceId, unitId, new Quantity(quantity));
        _receiptResources.Add(resource);
    }
    
    public void UpdateNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Номер документа не может быть пустым", nameof(number));
        Number = number;
    }
    
    public void UpdateDate(DateTime date)
    {
        Date = date;
    }
    
    public void ClearResources()
    {
        _receiptResources.Clear();
    }
    
    public void AddReceiptCreatedEvent()
    {
        var balanceDeltas = GetBalanceDeltas();
        AddDomainEvent(new ReceiptDocumentCreatedEvent(Id, balanceDeltas));
    }
    
    public void AddReceiptUpdatedEvent(IReadOnlyCollection<BalanceAdjustment> deltaAdjustments)
    {
        AddDomainEvent(new ReceiptDocumentUpdatedEvent(Id, deltaAdjustments));
    }
    
    public void AddReceiptDeletedEvent()
    {
        var balanceDeltas = GetBalanceDeltas();
        AddDomainEvent(new ReceiptDocumentDeletedEvent(Id, balanceDeltas));
    }
    
    private IReadOnlyCollection<BalanceAdjustment> GetBalanceDeltas()
    {
        return _receiptResources
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .Select(g => new BalanceAdjustment(g.Key.ResourceId, g.Key.UnitOfMeasureId, g.Sum(r => r.Quantity.Value)))
            .ToList();
    }
}