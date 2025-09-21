using MediatR;
using WarehouseManagement.Application.Common.Extensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
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
        var newResources = GetNewResources(document, command.Resources);
        if (newResources.Any())
        {
            await validationService.ValidateResourcesAsync(newResources.Select(r => r.ResourceId), ct);
            await validationService.ValidateUnitsAsync(newResources.Select(r => r.UnitId), ct);
        }
        
        // 4. Рассчитываем дельты для изменения балансов
        var deltas = CalculateBalanceDeltas(document, command.Resources);

        // 5. Обновляем документ
        UpdateDocumentResources(document, command);
        
        // 6. Add domain event to handle balance adjustments
        if (deltas.Any())
        {
            document.AddReceiptUpdatedEvent(deltas.ToDomainAdjustments().ToList());
        }

        receiptRepository.Update(document);
        await unitOfWork.SaveEntitiesAsync(ct);

        return Unit.Value;
    }

    private List<ReceiptResourceDto> GetNewResources(ReceiptDocument document, List<ReceiptResourceDto> commandResources)
    {
        var existingKeys = document.ReceiptResources
            .Select(r => new { r.ResourceId, r.UnitOfMeasureId })
            .ToHashSet();

        return commandResources
            .Where(r => !existingKeys.Contains(new { r.ResourceId, UnitOfMeasureId = r.UnitId }))
            .ToList();
    }

    private List<BalanceDelta> CalculateBalanceDeltas(ReceiptDocument document, List<ReceiptResourceDto> commandResources)
    {
        var oldQuantities = document.ReceiptResources
            .ToDictionary(g => new ResourceUnitKey(g.ResourceId, g.UnitOfMeasureId), r => r.Quantity.Value);
        
        var newQuantities = commandResources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new {r.ResourceId, r.UnitId})
            .ToDictionary(g => new ResourceUnitKey(g.Key.ResourceId, g.Key.UnitId), r => r.Sum(d => d.Quantity));
        
        var allKeys = oldQuantities.Keys.Union(newQuantities.Keys);
        return allKeys
            .Select(key => new BalanceDelta(
                key.ResourceId,
                key.UnitOfMeasureId,
                newQuantities.GetValueOrDefault(key, 0) - oldQuantities.GetValueOrDefault(key, 0)))
            .Where(d => d.Quantity != 0)
            .ToList();
    }

    private void UpdateDocumentResources(ReceiptDocument document, UpdateReceiptCommand command)
    {
        document.UpdateNumber(command.Number);
        document.UpdateDate(command.Date);
        document.ClearResources();

        var groupedResources = command.Resources.Where(r => r.Quantity > 0)
            .GroupBy(d => new { d.ResourceId, d.UnitId })
            .Select(c => new BalanceDelta(c.Key.ResourceId, c.Key.UnitId, c.Sum(d => d.Quantity))
            ).ToList();

        foreach (var groupedResource in groupedResources)
        {
            document.AddResource(groupedResource.ResourceId, groupedResource.UnitOfMeasureId, groupedResource.Quantity);
        }
    }
}