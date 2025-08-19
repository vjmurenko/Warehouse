using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocument(string number, DateTime date, List<ReceiptResource>? resources = null) : Entity, IAggregateRoot
{
    public string Number { get; set; } = !string.IsNullOrWhiteSpace(number) ? number.Trim() : throw new ArgumentNullException(nameof(number));
    public DateTime Date { get; set; } = date;
    public List<ReceiptResource> ReceiptResources { get; set; } = resources ?? [];

    public void AddResource(ReceiptResource resource)
    {
        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }
        ReceiptResources.Add(resource);
    }

    public void RemoveResource(ReceiptResource resource)
    {
        ReceiptResources.Remove(resource);
    }
}