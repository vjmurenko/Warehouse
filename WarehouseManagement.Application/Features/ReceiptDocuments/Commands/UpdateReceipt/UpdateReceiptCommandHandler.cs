using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    IReceiptValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateReceiptCommand, Unit>
{
    public async Task<Unit> Handle(UpdateReceiptCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Получение существующего документа
            var existingDocument = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (existingDocument == null)
                throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

            // 2. Проверка уникальности номера (исключая текущий документ)
            if (await receiptRepository.ExistsByNumberAsync(command.Number, command.Id, cancellationToken))
                throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

            // 3. Сохранение старых ресурсов для отката баланса
            var oldResources = existingDocument.ReceiptResources.ToList();

            // 4. Откат старых изменений баланса
            foreach (var oldResource in oldResources)
            {
                await balanceService.DecreaseBalance(
                    oldResource.ResourceId,
                    oldResource.UnitOfMeasureId,
                    oldResource.Quantity,
                    cancellationToken);
            }

            // 5. Валидация новых ресурсов
            foreach (var dto in command.Resources)
            {
                await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
                await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
            }

            // 6. Обновление документа через доменные методы
            existingDocument.UpdateNumber(command.Number);
            existingDocument.UpdateDate(command.Date);
            existingDocument.ClearResources();

            // 7. Добавление новых ресурсов
            foreach (var dto in command.Resources)
            {
                existingDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }

            // 8. Применение новых изменений баланса
            foreach (var newResource in existingDocument.ReceiptResources)
            {
                await balanceService.IncreaseBalance(
                    newResource.ResourceId,
                    newResource.UnitOfMeasureId,
                    newResource.Quantity,
                    cancellationToken);
            }

            // 9. Сохранение изменений
            await receiptRepository.UpdateAsync(existingDocument, cancellationToken);
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