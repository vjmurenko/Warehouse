using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Adapters;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;

public class UpdateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateShipmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateShipmentCommand command, CancellationToken cancellationToken)
    {
        // 1. Получение документа
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document == null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (document.IsSigned)
            throw new InvalidOperationException("Подписанный документ отгрузки нельзя редактировать. Используйте команду отзыва документа.");

        // 2. Проверка уникальности номера
        if (await shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        // 3. Валидация клиента
        await validationService.ValidateClient(command.ClientId, document.ClientId, cancellationToken);

        // 4. Валидация ресурсов до обновления
        await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken, document);
        
        // 5. Очистка и обновление ресурсов документа
        Updatedocument(document, command);

        document.ValidateNotEmpty();

        var delta = document.ShipmentResources.Select(r => new ShipmentResourceAdapter(r).ToDelta()).ToList();

        // 6. Проверка доступности балансов (для новых ресурсов)
        await balanceService.ValidateBalanceAvailability(delta, cancellationToken);

        // 7. Если подписываем, делаем списание баланса
        if (command.Sign)
        {
            await balanceService.DecreaseBalances(delta, cancellationToken);
            document.Sign();
        }

        // 8. Сохранение изменений
        shipmentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private void Updatedocument(ShipmentDocument document, UpdateShipmentCommand command)
    {
        document.UpdateNumber(command.Number);
        document.UpdateClientId(command.ClientId);
        document.UpdateDate(command.Date);
        document.ClearResources();
        foreach (var dto in command.Resources)
        {
            document.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
    }
}