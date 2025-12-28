using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        await validationService.ValidateClient(command.ClientId, ctx: cancellationToken);

        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken);

        var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);

        var resources = command.Resources.Select(r => ShipmentResource.Create(shipmentDocument.Id, r.ResourceId, r.UnitId, r.Quantity));
        shipmentDocument.SetResources(resources);

        shipmentDocument.ValidateNotEmpty();

        if (command.Sign)
        {
            shipmentDocument.Sign();
        }

        shipmentRepository.Create(shipmentDocument);

        await unitOfWork.SaveEntitiesAsync(cancellationToken);
        return shipmentDocument.Id;
    }
}
