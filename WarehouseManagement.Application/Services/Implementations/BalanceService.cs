using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class BalanceService(
    IBalanceRepository balanceRepository,
    IBalanceValidatorService validatorService,
    ILogger<BalanceService> logger) : IBalanceService
{
    public async Task IncreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx)
    {
        logger.LogInformation("Increasing balances for {DeltaCount} items", deltas.Count());

        var positiveDeltas = deltas.Select(d => d with { Quantity = Math.Abs(d.Quantity) });
        await AdjustBalances(positiveDeltas, ctx);

        logger.LogInformation("Successfully increased balances for {DeltaCount} items", deltas.Count());
    }

    public async Task DecreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx)
    {
        logger.LogInformation("Decreasing balances for {DeltaCount} items", deltas.Count());

        var negativeDeltas = deltas.Select(d => d with { Quantity = -Math.Abs(d.Quantity) });
        await AdjustBalances(negativeDeltas, ctx);

        logger.LogInformation("Successfully decreased balances for {DeltaCount} items", deltas.Count());
    }

    public async Task AdjustBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx)
    {
        var deltasList = deltas.Where(d => d.Quantity != 0).ToList();

        if (!deltasList.Any())
        {
            logger.LogInformation("No deltas to adjust, returning early");
            return;
        }

        logger.LogInformation("Adjusting balances for {DeltaCount} items", deltasList.Count);

        var keys = deltasList.Select(d => new ResourceUnitKey(d.ResourceId, d.UnitOfMeasureId));
        var balances = await balanceRepository.GetForUpdateAsync(keys, ctx);

        await ProcessDelta(deltasList, balances, ctx);

        logger.LogInformation("Successfully adjusted balances for {DeltaCount} items", deltasList.Count);
    }

    private async Task ProcessDelta(List<BalanceDelta> deltaList, IDictionary<ResourceUnitKey, Balance> balances, CancellationToken ctx)
    {
        foreach (var delta in deltaList)
        {
            var key = new ResourceUnitKey(delta.ResourceId, delta.UnitOfMeasureId);
            balances.TryGetValue(key, out var balance);

            if (delta.Quantity > 0)
            {
                await ApplyIncrease(delta, balance, ctx);
            }
            else
            {
                await ApplyDecrease(delta, balance!, balances, ctx);
            }

            if (balance is { Quantity.Value: 0 })
            {
                balanceRepository.Delete(balance);
            }
        }
    }

    private async Task ApplyIncrease(BalanceDelta delta, Balance? balance, CancellationToken ctx)
    {
        var qty = new Quantity(delta.Quantity);

        if (balance == null)
        {
            balance = new Balance(delta.ResourceId, delta.UnitOfMeasureId, qty);
            await balanceRepository.AddAsync(balance, ctx);
        }
        else
        {
            balance.Increase(qty);
        }
    }

    private async Task ApplyDecrease(BalanceDelta delta, Balance balance, IDictionary<ResourceUnitKey, Balance> balances, CancellationToken ctx)
    {
        await validatorService.ValidateBalanceAvailability([delta], balances, ctx);

        var decreaseAmount = new Quantity(Math.Abs(delta.Quantity));
        balance.Decrease(decreaseAmount);
    }
}