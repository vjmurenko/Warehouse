using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public sealed class ReceiptDocument : Entity, IAggregateRoot
{
    private readonly List<ReceiptResource> _receiptResources = new();
    public string Number { get; private set; }
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();

    public ReceiptDocument(string number, DateTime date)
    {
        Id = Guid.NewGuid();
        ArgumentException.ThrowIfNullOrEmpty(number);
        Number = number;
        Date = date;
    }

    public void AddResource(Guid resourceId, Guid unitId, decimal quantity)
    {
        _receiptResources.Add(new ReceiptResource(Id, resourceId, unitId, quantity));
    }

    public void UpdateNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number;
    }

    public void UpdateDate(DateTime date) => Date = date;

    public void ClearResources() => _receiptResources.Clear();

    public void Delete()
    {
        AddDomainEvent(new ReceiptDocumentDeletedEvent(Id));
    }

    public void SetResources(IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> resources)
    {
        _receiptResources.Clear();
        
        foreach (var (resourceId, unitId, qty) in resources.Where(r => r.Quantity > 0))
            AddResource(resourceId, unitId, qty);

        AddDomainEvent(new ReceiptDocumentCreatedEvent(Id));
    }

    public void UpdateResources(IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> newResources)
    {
        AddDomainEvent(new ReceiptDocumentUpdatedEvent(Id));
        
        _receiptResources.Clear();
        
        foreach (var (resourceId, unitId, qty) in newResources.Where(r => r.Quantity > 0))
            AddResource(resourceId, unitId, qty);
    }
}
