using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class BalanceService(IBalanceRepository balanceRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository) : IBalanceService
{
    public async Task IncreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        var positiveDeltas = deltas.Select(d => d with { Quantity = Math.Abs(d.Quantity) });
        await AdjustBalances(positiveDeltas, ct);
    }
    
    public async Task DecreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        var negativeDeltas = deltas.Select(d => d with { Quantity = -Math.Abs(d.Quantity) });
        await AdjustBalances(negativeDeltas, ct);
    }
    
    public async Task ValidateBalanceAvailability(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        await ValidateBalanceForDecrease(deltas, ct: ct);
    }
    
    public async Task AdjustBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        var deltasList = deltas.ToList();

        if (!deltasList.Any())
            return;

        var keys = deltasList.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
        var balances = await balanceRepository.GetForUpdateAsync(keys, ct);

        foreach (var delta in deltasList.Where(d => d.Quantity != 0))
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
                await ValidateBalanceForDecrease([delta], balances, ct);
                
                var decreaseAmount = new Quantity(Math.Abs(delta.Quantity));
                
                balance!.Decrease(decreaseAmount);
            }
        }
    }
    
    private async Task ValidateBalanceForDecrease(IEnumerable<BalanceDelta> deltas, 
        IDictionary<ResourceUnitKey, Balance>? preFetchedBalances = null,
        CancellationToken ct = default)
    {
        var deltasList = deltas
            .Select(c => c with { Quantity = Math.Abs(c.Quantity) })
            .ToList();
        
        if (!deltasList.Any())
            return;
            
        IDictionary<ResourceUnitKey, Balance> balances;
        if (preFetchedBalances != null)
        {
            balances = preFetchedBalances;
        }
        else
        {
            var keys = deltasList.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
            balances = await balanceRepository.GetForUpdateAsync(keys, ct);
        }
        
        foreach (var delta in deltasList)
        {
            var key = new ResourceUnitKey(delta.ResourceId, delta.UnitOfMeasureId);
            balances.TryGetValue(key, out var balance);
            
            var decreaseAmount = new Quantity(Math.Abs(delta.Quantity));
            
            if (balance == null || balance.Quantity.Value < decreaseAmount.Value)
            {
                var resourceName = (await resourceRepository.GetByIdAsync(delta.ResourceId, ct)).Name;
                var unitOfMeasureName = (await unitOfMeasureRepository.GetByIdAsync(delta.UnitOfMeasureId, ct)).Name;
                
                throw new InsufficientBalanceException(
                    resourceName,
                    unitOfMeasureName,
                    decreaseAmount.Value,
                    balance?.Quantity.Value ?? 0);
            }
        }
    }
}