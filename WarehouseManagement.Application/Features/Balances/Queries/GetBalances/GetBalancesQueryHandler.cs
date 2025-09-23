using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.Balances.Queries.GetBalances;

public class GetBalancesQueryHandler(
    IBalanceRepository balanceRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository
    ) : IRequestHandler<GetBalancesQuery, List<BalanceDto>>
{
    public async Task<List<BalanceDto>> Handle(GetBalancesQuery query, CancellationToken ctx)
    {
        var balances = await balanceRepository.GetFilteredAsync(
            query.ResourceIds,
            query.UnitIds,
            ctx);

        var balanceDtos = new List<BalanceDto>();
        var resources = (await resourceRepository.GetByIdsAsync(balances.Select(c => c.ResourceId), ctx)).ToList();
        var units = (await unitOfMeasureRepository.GetByIdsAsync(balances.Select(b => b.UnitOfMeasureId), ctx)).ToList();

        foreach (var balance in balances)
        {
            var resource = resources.FirstOrDefault(r => r.Id == balance.ResourceId);
            var unit = units.FirstOrDefault(u => u.Id == balance.UnitOfMeasureId);

            if (resource != null && unit != null)
            {
                balanceDtos.Add(new BalanceDto(
                    balance.Id,
                    balance.ResourceId,
                    resource.Name,
                    balance.UnitOfMeasureId,
                    unit.Name,
                    balance.Quantity.Value
                ));
            }
        }

        return balanceDtos;
    }
}