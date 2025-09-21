using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

public class ShipmentDocumentChangedResourcesEventHandler(
    IBalanceService balanceService,
    ILogger<ShipmentDocumentChangedResourcesEventHandler> logger)
    : INotificationHandler<ShipmentDocumentChangedResourcesEvent>
{
    public async Task Handle(ShipmentDocumentChangedResourcesEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ShipmentDocumentChangedResourcesEvent for document {DocumentId}", notification.DocumentId);
        
        await balanceService.ValidateBalanceAvailability(notification.BalanceDeltas, cancellationToken);
        
        logger.LogInformation("Successfully processed ShipmentDocumentChangedResourcesEvent for document {DocumentId}", notification.DocumentId);
    }
}