using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;

public class UpdateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IReceiptValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateShipmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateShipmentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Получение документа
            var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

            // 2. Проверка что подписанный документ нельзя редактировать (только отзывать)
            if (document.IsSigned)
                throw new InvalidOperationException("Подписанный документ отгрузки нельзя редактировать. Используйте команду отзыва документа.");

            // 3. Проверка уникальности номера (исключая текущий документ)
            if (await shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, cancellationToken))
                throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

            // 4. Валидация новых ресурсов до их добавления
            foreach (var dto in command.Resources)
            {
                await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
                await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
            }

            // 5. Обновление свойств документа
            document.UpdateNumber(command.Number);
            document.UpdateClientId(command.ClientId);
            document.UpdateDate(command.Date);

            // 6. Очистка и добавление новых ресурсов
            document.ClearResources();
            foreach (var dto in command.Resources)
            {
                document.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }
            
            // 6.1. Проверка что документ не пустой (бизнес-правило)
            document.ValidateNotEmpty();

            // 7. Подписание если требуется (с проверкой баланса)
            if (command.SignImmediately)
            {
                foreach (var resource in document.ShipmentResources)
                {
                    await balanceService.DecreaseBalance(
                        resource.ResourceId,
                        resource.UnitOfMeasureId,
                        resource.Quantity,
                        cancellationToken);
                }
                document.Sign();
            }

            // 8. Сохранение изменений
            await shipmentRepository.UpdateAsync(document, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Unit.Value;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}