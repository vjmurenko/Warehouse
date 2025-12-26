using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public sealed class ShipmentDocumentRevokedEventHandler(
    IBalanceService balanceService,
    ILogger<ShipmentDocumentRevokedEventHandler> logger)
    : INotificationHandler<ShipmentDocumentRevokedEvent>
{
    public async Task Handle(ShipmentDocumentRevokedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ShipmentDocumentRevokedEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.IncreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ShipmentDocumentRevokedEvent for document {DocumentId}", notification.DocumentId);
    }
}