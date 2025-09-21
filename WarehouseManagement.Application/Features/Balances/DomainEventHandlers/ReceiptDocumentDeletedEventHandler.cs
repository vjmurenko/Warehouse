using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public class ReceiptDocumentDeletedEventHandler(
    IBalanceService balanceService,
    ILogger<ReceiptDocumentDeletedEventHandler> logger)
    : INotificationHandler<ReceiptDocumentDeletedEvent>
{
    public async Task Handle(ReceiptDocumentDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ReceiptDocumentDeletedEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.DecreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ReceiptDocumentDeletedEvent for document {DocumentId}", notification.DocumentId);
    }
}