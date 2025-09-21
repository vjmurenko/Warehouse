using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        // 1. Проверка уникальности номера
        if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        // 2. Валидация клиента
        await validationService.ValidateClient(command.ClientId, ctx: cancellationToken);
        
        // 3. Валидация ресурсов
        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken);
        
        // 4. Создание документа
        var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);
        shipmentDocument.SetResources(command.Resources.Select(r => new BalanceDelta(r.ResourceId, r.UnitId, r.Quantity)));
        
        // 4.2. Проверка что документ не пустой
        shipmentDocument.ValidateNotEmpty();
        
        // 5. Подписание документа если требуется (domain event will handle balance validation and decrease)
        if (command.Sign)
        {
            shipmentDocument.Sign();
        }

        // 6. Сохранение документа
        shipmentRepository.Create(shipmentDocument);

        await unitOfWork.SaveEntitiesAsync(cancellationToken);
        return shipmentDocument.Id;
    }
}