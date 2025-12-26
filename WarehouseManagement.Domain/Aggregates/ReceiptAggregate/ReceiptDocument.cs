using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocument : Entity, IAggregateRoot
{
    private readonly List<ReceiptResource> _receiptResources = new();
    public string Number { get; private set; }
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();

    private ReceiptDocument()
    {
    }

    public ReceiptDocument(string number, DateTime date)
    {
        Id = Guid.NewGuid();
        
        ArgumentException.ThrowIfNullOrEmpty(number);
        Number = number;
        Date = date;
    }

    public void AddResource(Guid resourceId, Guid unitId, decimal quantity)
    {
        var resource = new ReceiptResource(Id, resourceId, unitId, new Quantity(quantity));
        _receiptResources.Add(resource);
    }

    public void UpdateNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
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
    
    public void AddReceiptUpdatedEvent(IReadOnlyCollection<BalanceDelta> deltaAdjustments)
    {
        AddDomainEvent(new ReceiptDocumentUpdatedEvent(Id, deltaAdjustments));
    }

    public void AddReceiptDeletedEvent()
    {
        var balanceDeltas = _receiptResources
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .Select(g => new BalanceDelta(g.Key.ResourceId, g.Key.UnitOfMeasureId, g.Sum(r => r.Quantity.Value)))
            .ToList();;
        AddDomainEvent(new ReceiptDocumentDeletedEvent(Id, balanceDeltas));
    }
    
    public void SetResources(IEnumerable<BalanceDelta> resources)
    {
        if (!resources.Any()) return;

        var balanceDeltas = resources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .Select(g => new BalanceDelta(g.Key.ResourceId, g.Key.UnitOfMeasureId, g.Sum(r => r.Quantity)))
            .ToList();

        _receiptResources.Clear();

        foreach (var bd in balanceDeltas)
        {
            AddResource(bd.ResourceId, bd.UnitOfMeasureId, bd.Quantity);
        }
        
        AddDomainEvent(new ReceiptDocumentCreatedEvent(Id, balanceDeltas));
    }

    public void UpdateResources(IEnumerable<BalanceDelta> newResources)
    {
        // старое состояние
        var oldQuantities = _receiptResources
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity.Value));

        // новое состояние
        var newQuantities = newResources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        // все ключи
        var allKeys = oldQuantities.Keys.Union(newQuantities.Keys);

        var deltas = allKeys
            .Select(key => new BalanceDelta(
                key.ResourceId,
                key.UnitOfMeasureId,
                newQuantities.GetValueOrDefault(key, 0) - oldQuantities.GetValueOrDefault(key, 0)))
            .Where(d => d.Quantity != 0)
            .ToList();

        // применяем новое состояние к документу
        _receiptResources.Clear();
        foreach (var kv in newQuantities)
        {
            AddResource(kv.Key.ResourceId, kv.Key.UnitOfMeasureId, kv.Value);
        }

        AddReceiptUpdatedEvent(deltas);
    }
}