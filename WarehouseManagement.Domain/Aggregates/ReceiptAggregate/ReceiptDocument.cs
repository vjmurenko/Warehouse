using WarehouseManagement.Domain.Events;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public sealed class ReceiptDocument : AggregateRoot<Guid>
{
    private readonly List<ReceiptResource> _receiptResources = [];

    public string Number { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();

    // EF Core constructor
    private ReceiptDocument(Guid id, string number, DateTime date) : base(id)
    {
        Number = number;
        Date = date;
    }
    
    private ReceiptDocument(Guid id, string number, DateTime date, IEnumerable<ReceiptResource> resources) 
        : this(id, number, date)
    {
        ArgumentException.ThrowIfNullOrEmpty(number);
        foreach (var resource in resources)
        {
            resource.SetReceiptDocumentId(id);
            _receiptResources.Add(resource);
        }
    }

    public static ReceiptDocument Create(string number, DateTime date, IEnumerable<ReceiptResource> resources)
    {
        var document = new ReceiptDocument(Guid.NewGuid(), number, date, resources);
        document.Raise(new ReceiptDocumentCreatedEvent(document.Id));
        
        return document;
    }
    
    public void Delete()
    {
        Raise(new ReceiptDocumentDeletedEvent(Id));
    }

    public void Update(string number, DateTime date, IEnumerable<ReceiptResource> newResources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        
        Number = number;
        Date = date;
        UpdateResources(newResources);
        
        Raise(new ReceiptDocumentUpdatedEvent(Id));
    }

    private void UpdateResources(IEnumerable<ReceiptResource> newResources)
    {
        _receiptResources.Clear();
        foreach (var resource in newResources.Where(r => r.Quantity > 0))
        {
            _receiptResources.Add(resource);
        }
    }
}
