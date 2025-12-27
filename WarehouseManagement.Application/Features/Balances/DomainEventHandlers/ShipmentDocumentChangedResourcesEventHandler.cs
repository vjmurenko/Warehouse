using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ShipmentDocumentChangedResourcesEventHandler(
    IShipmentRepository shipmentRepository,
    IStockService stockService)
    : INotificationHandler<ShipmentDocumentChangedResourcesEvent>
{
    public async Task Handle(ShipmentDocumentChangedResourcesEvent notification, CancellationToken ctx)
    {
        var shipment = await shipmentRepository.GetByIdWithResourcesAsync(notification.DocumentId, ctx);
        if (shipment is null) return;

        var items = shipment.ShipmentResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity))
            .ToList();

        await stockService.ValidateAvailability(items, ctx);
    }
}
