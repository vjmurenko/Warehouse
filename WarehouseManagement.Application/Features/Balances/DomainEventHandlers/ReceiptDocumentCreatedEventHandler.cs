using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public class ReceiptDocumentCreatedEventHandler(
    IBalanceService balanceService,
    ILogger<ReceiptDocumentCreatedEventHandler> logger)
    : INotificationHandler<ReceiptDocumentCreatedEvent>
{
    public async Task Handle(ReceiptDocumentCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ReceiptDocumentCreatedEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.IncreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ReceiptDocumentCreatedEvent for document {DocumentId}", notification.DocumentId);
    }
}