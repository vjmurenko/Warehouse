﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class ReceiptDocumentService(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    INamedEntityValidationService validationService) : IReceiptDocumentService
{
    public async Task ValidateReceiptRequestAsync(string number, List<ReceiptResourceDto> resources, Guid? excludeDocumentId = null, CancellationToken cancellationToken = default)
    {
        if (excludeDocumentId.HasValue)
        {
            if (await receiptRepository.ExistsByNumberAsync(number, excludeDocumentId.Value, cancellationToken))
                throw new InvalidOperationException($"Документ с номером {number} уже существует");
        }
        else
        {
            if (await receiptRepository.ExistsByNumberAsync(number))
                throw new InvalidOperationException($"Документ с номером {number} уже существует");
        }

        foreach (var dto in resources)
        {
            await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
            await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
        }
    }

    public async Task ApplyReceiptBalanceChangesAsync(ReceiptDocument document, CancellationToken cancellationToken)
    {
        foreach (var resource in document.ReceiptResources)
        {
            await balanceService.IncreaseBalance(
                resource.ResourceId,
                resource.UnitOfMeasureId,
                resource.Quantity,
                cancellationToken);
        }
    }

    public async Task RevertReceiptBalanceChangesAsync(ReceiptDocument document, CancellationToken cancellationToken)
    {
        foreach (var resource in document.ReceiptResources)
        {
            await balanceService.DecreaseBalance(
                resource.ResourceId,
                resource.UnitOfMeasureId,
                resource.Quantity,
                cancellationToken);
        }
    }

    public async Task ApplyBalanceChangesForUpdateAsync(
        List<(Guid ResourceId, Guid UnitId, decimal Quantity)> oldResources,
        IReadOnlyCollection<ReceiptResource> newResources, 
        CancellationToken cancellationToken)
    {
        var oldResourceMap = oldResources
            .GroupBy(r => new { ResourceId = r.ResourceId, UnitId = r.UnitId })
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        var newResourceMap = newResources
            .GroupBy(r => new { ResourceId = r.ResourceId, UnitId = r.UnitOfMeasureId })
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity.Value));

        var allResourceKeys = oldResourceMap.Keys.Union(newResourceMap.Keys);

        var decreaseValidations = allResourceKeys
            .Where(resourceKey => {
                var oldQuantity = oldResourceMap.GetValueOrDefault(resourceKey, 0);
                var newQuantity = newResourceMap.GetValueOrDefault(resourceKey, 0);
                return newQuantity - oldQuantity < 0;
            })
            .ToList();
            
        foreach (var resourceKey in decreaseValidations)
        {
            var oldQuantity = oldResourceMap.GetValueOrDefault(resourceKey, 0);
            var newQuantity = newResourceMap.GetValueOrDefault(resourceKey, 0);
            var delta = newQuantity - oldQuantity;
            await balanceService.ValidateBalanceAvailability(
                resourceKey.ResourceId,
                resourceKey.UnitId,
                new Quantity(Math.Abs(delta)),
                cancellationToken);
        }

        foreach (var resourceKey in allResourceKeys)
        {
            var oldQuantity = oldResourceMap.GetValueOrDefault(resourceKey, 0);
            var newQuantity = newResourceMap.GetValueOrDefault(resourceKey, 0);
            var delta = newQuantity - oldQuantity;

            if (delta != 0)
            {
                await balanceService.AdjustBalance(
                    resourceKey.ResourceId,
                    resourceKey.UnitId,
                    delta,
                    cancellationToken);
            }
        }
    }
}