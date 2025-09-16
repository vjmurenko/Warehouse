using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Adapters;

public record ShipmentResourceAdapter(ShipmentResource Resource)
{
    public BalanceDelta ToDelta() =>
        new(Resource.ResourceId, Resource.UnitOfMeasureId, Resource.Quantity.Value);
}