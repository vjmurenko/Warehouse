using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        // 1. Проверка уникальности номера
        if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        // 2. Валидация клиента
        await validationService.ValidateClient(command.ClientId);
        
        // 3. Создание документа
        var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);
        
        
        // 4. Валидация ресурсов
        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken);
        
        foreach (var dto in command.Resources)
        {
            shipmentDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
        
        // 4.2. Проверка что документ не пустой (бизнес-правило)
        shipmentDocument.ValidateNotEmpty();

        // 5. Проверка доступности баланса (без списания)
        foreach (var resource in shipmentDocument.ShipmentResources)
        {
            await balanceService.ValidateBalanceAvailability(
                resource.ResourceId,
                resource.UnitOfMeasureId,
                resource.Quantity,
                cancellationToken);
        }

        // 6. Подписание документа и списание с баланса если требуется
        if (command.Sign)
        {
            // Списание при подписании (проверка уже выполнена выше)
            foreach (var resource in shipmentDocument.ShipmentResources)
            {
                await balanceService.DecreaseBalance(
                    resource.ResourceId,
                    resource.UnitOfMeasureId,
                    resource.Quantity,
                    cancellationToken);
            }
            shipmentDocument.Sign();
        }

        // 7. Сохранение документа
        shipmentRepository.Create(shipmentDocument);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shipmentDocument.Id;
    }
}