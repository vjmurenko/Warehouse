using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.Adapters;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    INamedEntityValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateReceiptCommand, Unit>
{
    public async Task<Unit> Handle(UpdateReceiptCommand command, CancellationToken ct)
    {
        // 1. Получаем существующий документ
        var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, ct);
        if (document == null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        // 2. Проверяем уникальность номера
        if (await receiptRepository.ExistsByNumberAsync(command.Number, command.Id, ct))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        // 3. Валидируем только новые ресурсы (которые ещё не в документе)
        var existingKeys = document.ReceiptResources
            .Select(r => new { r.ResourceId, r.UnitOfMeasureId })
            .ToHashSet();

        var newResources = command.Resources
            .Where(r => !existingKeys.Contains(new { r.ResourceId, UnitOfMeasureId = r.UnitId }))
            .ToList();

        if (newResources.Any())
        {
            await validationService.ValidateResourcesAsync(newResources.Select(r => r.ResourceId), ct);
            await validationService.ValidateUnitsAsync(newResources.Select(r => r.UnitId), ct);
        }

        // 4. Суммируем дубли и рассчитываем дельты
        var oldQuantities = document.ReceiptResources
            .GroupBy(r => new ResourceUnitKey(r.ResourceId, r.UnitOfMeasureId))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity.Value));

        var newQuantities = command.Resources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new ResourceUnitKey(r.ResourceId, r.UnitId))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        var allKeys = oldQuantities.Keys.Union(newQuantities.Keys);
        var deltas = allKeys
            .Select(key => new BalanceDelta(
                key.ResourceId, 
                key.UnitOfMeasureId, 
                newQuantities.GetValueOrDefault(key, 0) - oldQuantities.GetValueOrDefault(key, 0)))
            .Where(d => d.Quantity != 0)
            .ToList();

        // 5. Применяем 
        if (deltas.Any())
        {
            await balanceService.ValidateBalanceAvailability(deltas, ct);
            await balanceService.AdjustBalances(deltas, ct);
        }

        // 6. Обновляем документ
        document.UpdateNumber(command.Number);
        document.UpdateDate(command.Date);
        document.ClearResources();

        var finalResources = command.Resources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new { r.ResourceId, r.UnitId })
            .Select(g => new { g.Key.ResourceId, g.Key.UnitId, Quantity = g.Sum(r => r.Quantity) });

        foreach (var r in finalResources)
            document.AddResource(r.ResourceId, r.UnitId, r.Quantity);

        receiptRepository.Update(document);
        await unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}