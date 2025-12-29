using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Events;

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
        var id = Guid.NewGuid();
        var document = new ReceiptDocument(id, number, date, resources);
        document.Raise(new ReceiptDocumentCreatedEvent(id));
        return document;
    }

    public void UpdateNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number;
    }

    public void UpdateDate(DateTime date) => Date = date;

    public void Delete()
    {
        Raise(new ReceiptDocumentDeletedEvent(Id));
    }

    public void UpdateResources(IEnumerable<ReceiptResource> newResources)
    {
        Raise(new ReceiptDocumentUpdatedEvent(Id));
        _receiptResources.Clear();
        foreach (var resource in newResources.Where(r => r.Quantity > 0))
        {
            resource.SetReceiptDocumentId(Id);
            _receiptResources.Add(resource);
        }
    }

    public void ClearResources() => _receiptResources.Clear();
}
