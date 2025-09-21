using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public class ShipmentDocumentSignedEventHandler(
    IBalanceService balanceService,
    ILogger<ShipmentDocumentSignedEventHandler> logger)
    : INotificationHandler<ShipmentDocumentSignedEvent>
{
    public async Task Handle(ShipmentDocumentSignedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ShipmentDocumentSignedEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.DecreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ShipmentDocumentSignedEvent for document {DocumentId}", notification.DocumentId);
    }
}