using MediatR;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ShipmentDocumentRevokedEventHandler(IStockService stockService)
    : INotificationHandler<ShipmentDocumentRevokedEvent>
{
    public async Task Handle(ShipmentDocumentRevokedEvent notification, CancellationToken ctx)
    {
        await stockService.ReverseMovements(notification.DocumentId, ctx);
    }
}
