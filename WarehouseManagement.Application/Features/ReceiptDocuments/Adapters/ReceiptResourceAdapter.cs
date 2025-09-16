using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Adapters;

public record ReceiptResourceAdapter(ReceiptResource Resource)
{
    public BalanceDelta ToDelta() =>
        new(Resource.ResourceId, Resource.UnitOfMeasureId, Resource.Quantity.Value);
}