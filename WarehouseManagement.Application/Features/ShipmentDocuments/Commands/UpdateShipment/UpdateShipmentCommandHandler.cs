using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;

public sealed class UpdateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateShipmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateShipmentCommand command, CancellationToken cancellationToken)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (document.IsSigned)
            throw new InvalidOperationException("Подписанный документ отгрузки нельзя редактировать. Используйте команду отзыва документа.");

        if (await shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        await validationService.ValidateClient(command.ClientId, document.ClientId, cancellationToken);

        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken, document);

        document.UpdateNumber(command.Number);
        document.UpdateClientId(command.ClientId);
        document.UpdateDate(command.Date);
        document.SetResources(command.Resources.Select(r => (r.ResourceId, r.UnitId, r.Quantity)));

        document.ValidateNotEmpty();

        if (command.Sign)
        {
            document.Sign();
        }

        shipmentRepository.Update(document);
        await unitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
