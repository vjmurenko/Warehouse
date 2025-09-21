using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IBalanceValidatorService
{
    Task ValidateBalanceAvailability(
        IEnumerable<BalanceDelta> deltas,
        IDictionary<ResourceUnitKey, Balance>? preFetchedBalances = null,
        CancellationToken ctx = default);
}