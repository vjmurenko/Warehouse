using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ReceiptDocumentCreatedEventHandler(
    IReceiptRepository receiptRepository,
    IStockService stockService)
    : INotificationHandler<ReceiptDocumentCreatedEvent>
{
    public async Task Handle(ReceiptDocumentCreatedEvent notification, CancellationToken ctx)
    {
        var receipt = await receiptRepository.GetByIdWithResourcesAsync(notification.DocumentId, ctx);
        if (receipt is null) return;

        var items = receipt.ReceiptResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity));

        await stockService.RecordMovements(receipt.Id, MovementType.Receipt, items, ctx);
    }
}
