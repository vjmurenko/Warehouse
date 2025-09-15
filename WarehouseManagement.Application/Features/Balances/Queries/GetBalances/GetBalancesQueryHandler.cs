using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Balances.Queries.GetBalances;

public class GetBalancesQueryHandler(
    IBalanceRepository balanceRepository,
    IResourceService resourceService,
    IUnitOfMeasureService unitOfMeasureService) : IRequestHandler<GetBalancesQuery, List<BalanceDto>>
{
    public async Task<List<BalanceDto>> Handle(GetBalancesQuery query, CancellationToken ctx)
    {
        var balances = await balanceRepository.GetFilteredAsync(
            query.ResourceIds,
            query.UnitIds,
            ctx);

        var balanceDtos = new List<BalanceDto>();

        foreach (var balance in balances)
        {
            var resource = await resourceService.GetByIdAsync(balance.ResourceId, ctx);
            var unit = await unitOfMeasureService.GetByIdAsync(balance.UnitOfMeasureId, ctx);

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