using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.SignShipment;

public class SignShipmentHandler : IRequestHandler<SignShipmentCommand, bool>
{
    private readonly WarehouseDbContext _context;

    public SignShipmentHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SignShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipmentDocument = await _context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentDocumentId, cancellationToken);

        if (shipmentDocument == null)
        {
            throw new InvalidOperationException("Shipment document not found.");
        }

        if (shipmentDocument.IsSigned)
        {
            throw new InvalidOperationException("Shipment document is already signed.");
        }

        // Проверка наличия достаточного количества ресурсов на складе
        await CheckBalanceAvailability(shipmentDocument, cancellationToken);

        // Подписание документа
        shipmentDocument.Sign();

        // Обновление баланса
        await UpdateBalanceForShipment(shipmentDocument, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task CheckBalanceAvailability(ShipmentDocument shipmentDocument, CancellationToken cancellationToken)
    {
        foreach (var shipmentResource in shipmentDocument.ShipmentResources)
        {
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b =>
                    b.ResourceId == shipmentResource.ResourceId &&
                    b.UnitOfMeasureId == shipmentResource.UnitOfMeasureId,
                    cancellationToken);

            if (balance == null || balance.Quantity.Value < shipmentResource.Quantity.Value)
            {
                throw new InvalidOperationException(
                    $"Insufficient balance for resource {shipmentResource.ResourceId}. " +
                    $"Required: {shipmentResource.Quantity.Value}, Available: {balance?.Quantity.Value ?? 0}");
            }
        }
    }

    private async Task UpdateBalanceForShipment(ShipmentDocument shipmentDocument, CancellationToken cancellationToken)
    {
        foreach (var shipmentResource in shipmentDocument.ShipmentResources)
        {
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b =>
                    b.ResourceId == shipmentResource.ResourceId &&
                    b.UnitOfMeasureId == shipmentResource.UnitOfMeasureId,
                    cancellationToken);

            if (balance != null)
            {
                balance.Decrease(shipmentResource.Quantity);
            }
        }
    }
}
