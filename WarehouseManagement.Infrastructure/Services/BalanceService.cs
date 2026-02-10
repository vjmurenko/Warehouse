using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.BalanceAggregate;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Infrastructure.Services;

public sealed class BalanceService(
    IBalanceRepository balanceRepository,
    IReferenceRepository<Resource> resourceRepository,
    IReferenceRepository<UnitOfMeasure> unitRepository) : IBalanceService
{
    public async Task UpdateBalances(
        IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> items,
        CancellationToken ctx)
    {
        var list = items.Where(i => i.Quantity != 0).ToList();
        if (list.Count == 0) return;

        var keys = list.Select(i => (i.ResourceId, i.UnitId)).ToList();
        var balances = await balanceRepository.GetForUpdateAsync(keys, ctx);
        var balanceDict = balances.ToDictionary(b => (b.ResourceId, b.UnitOfMeasureId));

        foreach (var (resourceId, unitId, quantity) in list)
        {
            if (balanceDict.TryGetValue((resourceId, unitId), out var balance))
            {
                balance.AddQuantity(quantity);
            }
            else
            {
                var newBalance = Balance.Create(resourceId, unitId, quantity);
                balanceRepository.Create(newBalance);
            }
        }
    }

    public async Task ValidateAvailability(
        IEnumerable<(Guid ResourceId, Guid UnitId, decimal Required)> items,
        CancellationToken ctx)
    {
        var list = items.Where(i => i.Required > 0).ToList();
        if (list.Count == 0) return;

        var keys = list.Select(i => (i.ResourceId, i.UnitId)).ToList();
        var balances = await balanceRepository.GetForUpdateAsync(keys, ctx);
        var balanceDict = balances.ToDictionary(b => (b.ResourceId, b.UnitOfMeasureId), b => b.Quantity);

        foreach (var (resourceId, unitId, required) in list)
        {
            var available = balanceDict.GetValueOrDefault((resourceId, unitId), 0);
            
            if (available < required)
            {
                var resource = await resourceRepository.GetByIdAsync(resourceId, ctx);
                var unit = await unitRepository.GetByIdAsync(unitId, ctx);
                
                throw new InsufficientBalanceException(
                    resource?.Name ?? "Unknown",
                    unit?.Name ?? "Unknown",
                    required,
                    available);
            }
        }
    }
}
