using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class BalanceService(IBalanceRepository balanceRepository) : IBalanceService
{
    public async Task IncreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        
        var positiveDeltas = deltas.Select(d => d with { Quantity = Math.Abs(d.Quantity) });
        await AdjustBalances(positiveDeltas, ct);
    }
    
    public async Task DecreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        // Все дельты считаем отрицательными
        var negativeDeltas = deltas.Select(d => d with { Quantity = -Math.Abs(d.Quantity) });
        await AdjustBalances(negativeDeltas, ct);
    }
    
    public async Task ValidateBalanceAvailability(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        var aggregated = deltas
            .GroupBy(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId))
            .Select(g => new BalanceDelta(
                g.Key.ResourceId,
                g.Key.UnitOfMeasureId,
                g.Sum(x => x.Quantity)))
            .Where(d => d.Quantity > 0) // проверяем только положительные дельты
            .ToList();

        if (!aggregated.Any())
            return;

        var keys = aggregated.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
        var balances = await balanceRepository.GetForUpdateAsync(keys, ct);

        foreach (var delta in aggregated)
        {
            var key = new ResourceUnitKey(delta.ResourceId, delta.UnitOfMeasureId);
            balances.TryGetValue(key, out var balance);

            if (balance == null || balance.Quantity.Value < delta.Quantity)
            {
                throw new InsufficientBalanceException(
                    "Resource", 
                    "Unit",
                    delta.Quantity,
                    balance?.Quantity.Value ?? 0);
            }
        }
    }
    
    public async Task AdjustBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        var aggregated = deltas
            .GroupBy(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId))
            .Select(g => new BalanceDelta(
                g.Key.ResourceId,
                g.Key.UnitOfMeasureId,
                g.Sum(x => x.Quantity)))
            .Where(d => d.Quantity != 0)
            .ToList();

        if (!aggregated.Any())
            return;

        var keys = aggregated.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
        var balances = await balanceRepository.GetForUpdateAsync(keys, ct);

        foreach (var delta in aggregated)
        {
            var key = new ResourceUnitKey(delta.ResourceId, delta.UnitOfMeasureId);
            balances.TryGetValue(key, out var balance);

            if (delta.Quantity > 0)
            {
                var qty = new Quantity(delta.Quantity);

                if (balance == null)
                {
                    balance = new Balance(delta.ResourceId, delta.UnitOfMeasureId, qty);
                    await balanceRepository.AddAsync(balance, ct);
                }
                else
                {
                    balance.Increase(qty);
                }
            }
            else
            {
                var decreaseAmount = new Quantity(Math.Abs(delta.Quantity));

                if (balance == null || balance.Quantity.Value < decreaseAmount.Value)
                {
                    throw new InsufficientBalanceException(
                        "Resource",
                        "Unit",
                        decreaseAmount.Value,
                        balance?.Quantity.Value ?? 0);
                }

                balance.Decrease(decreaseAmount);
            }
        }
    }
}