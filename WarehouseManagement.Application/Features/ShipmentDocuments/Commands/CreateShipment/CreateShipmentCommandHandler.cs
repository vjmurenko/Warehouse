using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Adapters;
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
        await validationService.ValidateClient(command.ClientId, ctx: cancellationToken);
        
        // 3. Валидация ресурсов
        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken);
        
        // 4. Создание документа
        var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);
        foreach (var dto in command.Resources)
        {
            shipmentDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
        
        // 4.2. Проверка что документ не пустой (бизнес-правило)
        shipmentDocument.ValidateNotEmpty();

        // 5. Проверка доступности баланса (без списания)
        var deltas = shipmentDocument.ShipmentResources.Select(r => new ShipmentResourceAdapter(r).ToDelta()).ToList();
        await balanceService.ValidateBalanceAvailability(deltas, cancellationToken);
        
        // 6. Подписание документа и списание с баланса если требуется
        if (command.Sign)
        {
            await balanceService.DecreaseBalances(deltas, cancellationToken);
            shipmentDocument.Sign();
        }

        // 7. Сохранение документа
        shipmentRepository.Create(shipmentDocument);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shipmentDocument.Id;
    }
}