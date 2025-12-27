using MediatR;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ReceiptDocumentDeletedEventHandler(IStockService stockService)
    : INotificationHandler<ReceiptDocumentDeletedEvent>
{
    public async Task Handle(ReceiptDocumentDeletedEvent notification, CancellationToken ctx)
    {
        await stockService.ReverseMovements(notification.DocumentId, ctx);
    }
}
