using MediatR;
using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public sealed class RevokeShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeShipmentCommand, Unit>
{
    public async Task<Unit> Handle(RevokeShipmentCommand command, CancellationToken cancellationToken)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (!document.IsSigned)
            throw new InvalidOperationException("Документ не подписан и не может быть отозван");

        document.Revoke();

        shipmentRepository.Update(document);

        await unitOfWork.SaveEntitiesAsync(cancellationToken);
        return Unit.Value;
    }
}