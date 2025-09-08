using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    INamedEntityValidationService validationService,
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

            // 3. Создание карт старых и новых ресурсов для вычисления дельты
            var oldResourceMap = existingDocument.ReceiptResources
                .GroupBy(r => new { r.ResourceId, r.UnitOfMeasureId })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity.Value));

            var newResourceMap = command.Resources
                .GroupBy(r => new { r.ResourceId, UnitOfMeasureId = r.UnitId })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

            // 4. Получение всех уникальных комбинаций ресурс-единица
            var allResourceKeys = oldResourceMap.Keys.Union(newResourceMap.Keys).ToList();

            // 5. Валидация новых ресурсов (только для тех, которых не было в старом документе)
            foreach (var dto in command.Resources
                         .Where(c => !oldResourceMap.ContainsKey(new { c.ResourceId, UnitOfMeasureId = c.UnitId })))
            {
                await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
                await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
            }

            // 6. Проверка дельты и предварительная валидация баланса для уменьшений
            foreach (var resourceKey in allResourceKeys)
            {
                var oldQuantity = oldResourceMap.GetValueOrDefault(resourceKey, 0);
                var newQuantity = newResourceMap.GetValueOrDefault(resourceKey, 0);
                var delta = newQuantity - oldQuantity;

                // Если дельта отрицательная (уменьшение), нужно проверить, что баланс не станет отрицательным
                if (delta < 0)
                {
                    var decreaseAmount = new Quantity(Math.Abs(delta));
                    await balanceService.ValidateBalanceAvailability(
                        resourceKey.ResourceId,
                        resourceKey.UnitOfMeasureId,
                        decreaseAmount,
                        cancellationToken);
                }
            }

            // 7. Обновление документа через доменные методы
            existingDocument.UpdateNumber(command.Number);
            existingDocument.UpdateDate(command.Date);
            existingDocument.ClearResources();

            // 8. Добавление новых ресурсов
            foreach (var dto in command.Resources)
            {
                existingDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }

            // 9. Применение дельта изменений баланса
            foreach (var resourceKey in allResourceKeys)
            {
                var oldQuantity = oldResourceMap.GetValueOrDefault(resourceKey, 0);
                var newQuantity = newResourceMap.GetValueOrDefault(resourceKey, 0);
                var delta = newQuantity - oldQuantity;

                await balanceService.AdjustBalance(
                    resourceKey.ResourceId,
                    resourceKey.UnitOfMeasureId,
                    delta,
                    cancellationToken);
            }

            // 10. Сохранение изменений
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