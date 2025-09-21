using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IBalanceService
{
     Task IncreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx);
     Task DecreaseBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx);
     Task AdjustBalances(IEnumerable<BalanceDelta> deltas, CancellationToken ctx);
}