using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Domain.Aggregates.BalanceAggregate;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.Balances.Queries.GetBalances;

public sealed class GetBalancesQueryHandler(
    IBalanceRepository balanceRepository,
    IReferenceRepository<Resource> resourceRepository,
    IReferenceRepository<UnitOfMeasure> unitOfMeasureRepository
    ) : IRequestHandler<GetBalancesQuery, List<BalanceDto>>
{
    public async Task<List<BalanceDto>> Handle(GetBalancesQuery query, CancellationToken ctx)
    {
        var balances = await balanceRepository.GetFilteredAsync(
            query.ResourceIds,
            query.UnitIds,
            ctx);

        if (balances.Count == 0)
            return [];

        var resourceIds = balances.Select(b => b.ResourceId).Distinct();
        var unitIds = balances.Select(b => b.UnitOfMeasureId).Distinct();

        var resources = (await resourceRepository.GetByIdsAsync(resourceIds, ctx)).ToDictionary(r => r.Id);
        var units = (await unitOfMeasureRepository.GetByIdsAsync(unitIds, ctx)).ToDictionary(u => u.Id);

        return balances
            .Where(b => resources.ContainsKey(b.ResourceId) && units.ContainsKey(b.UnitOfMeasureId))
            .Select(b => new BalanceDto(
                b.Id,
                b.ResourceId,
                resources[b.ResourceId].Name,
                b.UnitOfMeasureId,
                units[b.UnitOfMeasureId].Name,
                b.Quantity))
            .ToList();
    }
}