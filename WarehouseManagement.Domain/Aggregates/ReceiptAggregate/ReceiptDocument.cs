using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public sealed class ReceiptDocument : AggregateRoot<Guid>
{
    private readonly List<ReceiptResource> _receiptResources = [];

    public string Number { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();

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
        return new ReceiptDocument(Guid.NewGuid(), number, date, resources);
    }

    public void Update(string number, DateTime date, IEnumerable<ReceiptResource> newResources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        
        Number = number;
        Date = date;
        UpdateResources(newResources);
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
