using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Extensions;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.Features.Balances.DomainEventHandlers;

// Receipt document event handlers
public class ReceiptDocumentCreatedEventHandler : INotificationHandler<ReceiptDocumentCreatedEvent>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ReceiptDocumentCreatedEventHandler> _logger;

    public ReceiptDocumentCreatedEventHandler(
        IBalanceService balanceService,
        ILogger<ReceiptDocumentCreatedEventHandler> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Handle(ReceiptDocumentCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ReceiptDocumentCreatedEvent for document {DocumentId}", notification.DocumentId);
        
        await _balanceService.IncreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        _logger.LogInformation("Successfully processed ReceiptDocumentCreatedEvent for document {DocumentId}", notification.DocumentId);
    }
}

public class ReceiptDocumentUpdatedEventHandler : INotificationHandler<ReceiptDocumentUpdatedEvent>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ReceiptDocumentUpdatedEventHandler> _logger;

    public ReceiptDocumentUpdatedEventHandler(
        IBalanceService balanceService,
        ILogger<ReceiptDocumentUpdatedEventHandler> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Handle(ReceiptDocumentUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ReceiptDocumentUpdatedEvent for document {DocumentId}", notification.DocumentId);
        
        await _balanceService.AdjustBalances(notification.BalanceDeltas, cancellationToken);
        
        _logger.LogInformation("Successfully processed ReceiptDocumentUpdatedEvent for document {DocumentId}", notification.DocumentId);
    }
}

public class ReceiptDocumentDeletedEventHandler : INotificationHandler<ReceiptDocumentDeletedEvent>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ReceiptDocumentDeletedEventHandler> _logger;

    public ReceiptDocumentDeletedEventHandler(
        IBalanceService balanceService,
        ILogger<ReceiptDocumentDeletedEventHandler> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Handle(ReceiptDocumentDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ReceiptDocumentDeletedEvent for document {DocumentId}", notification.DocumentId);
        
        await _balanceService.DecreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        _logger.LogInformation("Successfully processed ReceiptDocumentDeletedEvent for document {DocumentId}", notification.DocumentId);
    }
}

// Shipment document event handlers
public class ShipmentDocumentSignedEventHandler : INotificationHandler<ShipmentDocumentSignedEvent>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ShipmentDocumentSignedEventHandler> _logger;

    public ShipmentDocumentSignedEventHandler(
        IBalanceService balanceService,
        ILogger<ShipmentDocumentSignedEventHandler> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Handle(ShipmentDocumentSignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ShipmentDocumentSignedEvent for document {DocumentId}", notification.DocumentId);
        
        // First validate that balance is available
        await _balanceService.ValidateBalanceAvailability(notification.BalanceDeltas, cancellationToken);
        
        // Then decrease the balance
        await _balanceService.DecreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        _logger.LogInformation("Successfully processed ShipmentDocumentSignedEvent for document {DocumentId}", notification.DocumentId);
    }
}

public class ShipmentDocumentRevokedEventHandler : INotificationHandler<ShipmentDocumentRevokedEvent>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ShipmentDocumentRevokedEventHandler> _logger;

    public ShipmentDocumentRevokedEventHandler(
        IBalanceService balanceService,
        ILogger<ShipmentDocumentRevokedEventHandler> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Handle(ShipmentDocumentRevokedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ShipmentDocumentRevokedEvent for document {DocumentId}", notification.DocumentId);
        
        await _balanceService.IncreaseBalances(notification.BalanceDeltas, cancellationToken);
        
        _logger.LogInformation("Successfully processed ShipmentDocumentRevokedEvent for document {DocumentId}", notification.DocumentId);
    }
}