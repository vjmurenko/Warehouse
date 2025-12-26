using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class BalanceValidatorService (IBalanceRepository balanceRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository,
    ILogger<BalanceService> logger) : IBalanceValidatorService
{
    
    public async Task ValidateBalanceAvailability(IEnumerable<BalanceDelta> deltas,
        IDictionary<ResourceUnitKey, Balance>? preFetchedBalances = null,
        CancellationToken ctx = default)
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
        var balances = preFetchedBalances ?? await balanceRepository.GetForUpdateAsync(deltasList.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId)), ctx);

        foreach (var delta in deltasList)
        {
            var key = new ResourceUnitKey(delta.ResourceId, delta.UnitOfMeasureId);
            balances.TryGetValue(key, out var balance);
            
            var decreaseAmount = new Quantity(Math.Abs(delta.Quantity));
            
            if (balance is null || balance.Quantity.Value < decreaseAmount.Value)
            {
                logger.LogWarning("Insufficient balance for resource {ResourceId} and unit {UnitOfMeasureId}. Required: {Required}, Available: {Available}",
                    delta.ResourceId, delta.UnitOfMeasureId, decreaseAmount.Value, balance?.Quantity.Value ?? 0);
                
                var resourceName = await GetResourceName(delta.ResourceId, ctx);
                var unitOfMeasureName = await GetUnitOfMeasureName(delta.UnitOfMeasureId, ctx);
                
                throw new InsufficientBalanceException(
                    resourceName,
                    unitOfMeasureName,
                    decreaseAmount.Value,
                    balance?.Quantity.Value ?? 0);
            }
        }
        logger.LogInformation("Successfully validated balance for decrease for {DeltaCount} items", deltas.Count());
    }
    
    private async Task<string> GetResourceName(Guid resourceId, CancellationToken ctx)
    {
        return (await resourceRepository.GetByIdAsync(resourceId, ctx)).Name;
    }
    
    private async Task<string> GetUnitOfMeasureName(Guid unitOfMeasureId, CancellationToken ctx)
    {
        return (await unitOfMeasureRepository.GetByIdAsync(unitOfMeasureId, ctx)).Name;
    }
}