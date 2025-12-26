using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ReceiptDocumentUpdatedEventHandler(
    IBalanceService balanceService,
    ILogger<ReceiptDocumentUpdatedEventHandler> logger)
    : INotificationHandler<ReceiptDocumentUpdatedEvent>
{
    public async Task Handle(ReceiptDocumentUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ReceiptDocumentUpdatedEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.AdjustBalances(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ReceiptDocumentUpdatedEvent for document {DocumentId}", notification.DocumentId);
    }
}