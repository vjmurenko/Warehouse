using Microsoft.Extensions.Logging;
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
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository,
    ILogger<BalanceService> logger) : IBalanceService
{
    public async Task IncreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        logger.LogInformation("Increasing balances for {DeltaCount} items", deltas.Count());
        
        var positiveDeltas = deltas.Select(d => d with { Quantity = Math.Abs(d.Quantity) });
        await AdjustBalances(positiveDeltas, ct);
        
        logger.LogInformation("Successfully increased balances for {DeltaCount} items", deltas.Count());
    }
    
    public async Task DecreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        logger.LogInformation("Decreasing balances for {DeltaCount} items", deltas.Count());
        
        var negativeDeltas = deltas.Select(d => d with { Quantity = -Math.Abs(d.Quantity) });
        await AdjustBalances(negativeDeltas, ct);
        
        logger.LogInformation("Successfully decreased balances for {DeltaCount} items", deltas.Count());
    }
    
    public async Task ValidateBalanceAvailability(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        logger.LogInformation("Validating balance availability for {DeltaCount} items", deltas.Count());
        
        await ValidateBalanceForDecrease(deltas, ct: ct);
        
        logger.LogInformation("Successfully validated balance availability for {DeltaCount} items", deltas.Count());
    }
    
    public async Task AdjustBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ct)
    {
        logger.LogInformation("Adjusting balances for {DeltaCount} items", deltas.Count());
        
        var deltasList = deltas.ToList();

        if (!deltasList.Any())
        {
            logger.LogInformation("No deltas to adjust, returning early");
            return;
        }

        var keys = deltasList.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
        
        logger.LogInformation("Retrieving balances for {KeyCount} keys", keys.Count());
        
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
        logger.LogInformation("Successfully adjusted balances for {DeltaCount} items", deltas.Count());
    }
    
    private async Task ValidateBalanceForDecrease(IEnumerable<BalanceDelta> deltas,
        IDictionary<ResourceUnitKey, Balance>? preFetchedBalances = null,
        CancellationToken ct = default)
    {
        logger.LogInformation("Validating balance for decrease for {DeltaCount} items", deltas.Count());
        var deltasList = deltas
            .Select(c => c with { Quantity = Math.Abs(c.Quantity) })
            .ToList();
        
        if (!deltasList.Any())
        {
            logger.LogInformation("No deltas to validate, returning early");
            return;
        }
            
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
                logger.LogWarning("Insufficient balance for resource {ResourceId} and unit {UnitOfMeasureId}. Required: {Required}, Available: {Available}",
                    delta.ResourceId, delta.UnitOfMeasureId, decreaseAmount.Value, balance?.Quantity.Value ?? 0);
                
                var resourceName = (await resourceRepository.GetByIdAsync(delta.ResourceId, ct)).Name;
                var unitOfMeasureName = (await unitOfMeasureRepository.GetByIdAsync(delta.UnitOfMeasureId, ct)).Name;
                
                throw new InsufficientBalanceException(
                    resourceName,
                    unitOfMeasureName,
                    decreaseAmount.Value,
                    balance?.Quantity.Value ?? 0);
            }
        }
        logger.LogInformation("Successfully validated balance for decrease for {DeltaCount} items", deltas.Count());
    }
}