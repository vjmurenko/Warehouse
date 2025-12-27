using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ShipmentDocumentSignedEventHandler(
    IShipmentRepository shipmentRepository,
    IStockService stockService)
    : INotificationHandler<ShipmentDocumentSignedEvent>
{
    public async Task Handle(ShipmentDocumentSignedEvent notification, CancellationToken ctx)
    {
        var shipment = await shipmentRepository.GetByIdWithResourcesAsync(notification.DocumentId, ctx);
        if (shipment is null) return;

        var items = shipment.ShipmentResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity))
            .ToList();

        await stockService.ValidateAvailability(items, ctx);

        var negativeItems = items.Select(i => (i.ResourceId, i.UnitOfMeasureId, -i.Quantity));
        await stockService.RecordMovements(shipment.Id, MovementType.Shipment, negativeItems, ctx);
    }
}
