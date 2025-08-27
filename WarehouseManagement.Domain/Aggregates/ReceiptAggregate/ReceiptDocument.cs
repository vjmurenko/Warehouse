using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocument : Entity, IAggregateRoot
{
    private readonly List<ReceiptResource> _receiptResources = new();
    public string Number { get; private set; }
    public DateTime Date { get; private set; }
    public IReadOnlyCollection<ReceiptResource> ReceiptResources => _receiptResources.AsReadOnly();
    
    // Конструктор для EF
    private ReceiptDocument() { }

    public ReceiptDocument(string number, DateTime date)
    {
        Id = Guid.NewGuid(); 
        Number = number ?? throw new ArgumentNullException(nameof(number));
        Date = date;
    }
    
    public void AddResource(Guid resourceId, Guid unitId, decimal quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше 0");

        var resource = new ReceiptResource(Id, resourceId, unitId, new Quantity(quantity));
        _receiptResources.Add(resource);
    }
}