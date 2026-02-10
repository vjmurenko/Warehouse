using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Features.Balances.Queries.GetBalances;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.Balances;

public sealed class GetBalancesQueryHandler(WarehouseDbContext context) : IRequestHandler<GetBalancesQuery, List<BalanceDto>>
{
    public async Task<List<BalanceDto>> Handle(GetBalancesQuery request, CancellationToken ctx)
    {
        var query = context.Balances
            .AsNoTracking()
            .AsSplitQuery()
            .AsQueryable();

        if (request.ResourceIds is not null && request.ResourceIds.Count > 0)
            query = query.Where(b => request.ResourceIds.Contains(b.ResourceId));

        if (request.UnitIds is not null && request.UnitIds.Count > 0)
            query = query.Where(b => request.UnitIds.Contains(b.UnitOfMeasureId));

        return await query
            .Where(b => b.Quantity > 0)
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .Select(b => new BalanceDto(b.Id, b.ResourceId, b.Resource.Name, b.UnitOfMeasureId, b.UnitOfMeasure.Name, b.Quantity))
            .ToListAsync(ctx);
    }
}