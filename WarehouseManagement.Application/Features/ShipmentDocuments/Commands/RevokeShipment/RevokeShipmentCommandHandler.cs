using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public sealed class RevokeShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeShipmentCommand, Unit>
{
    public async Task<Unit> Handle(RevokeShipmentCommand command, CancellationToken cancellationToken)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (!document.IsSigned)
            throw new InvalidOperationException("Документ не подписан и не может быть отозван");

        var items = document.ShipmentResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity));
        await balanceService.UpdateBalances(items, cancellationToken);

        document.Revoke();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}