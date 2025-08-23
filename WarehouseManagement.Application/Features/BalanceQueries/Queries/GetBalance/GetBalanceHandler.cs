using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.BalanceQueries.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Application.Features.BalanceQueries.Queries.GetBalance;

public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, List<BalanceDto>>
{
    private readonly WarehouseDbContext _context;

    public GetBalanceHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<List<BalanceDto>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Balance>()
            .Include(b => _context.Set<Resource>().Where(r => r.Id == b.ResourceId).FirstOrDefault())
            .Include(b => _context.Set<UnitOfMeasure>().Where(u => u.Id == b.UnitOfMeasureId).FirstOrDefault())
            .AsQueryable();

        if (request.ResourceIds != null && request.ResourceIds.Any())
        {
            query = query.Where(b => request.ResourceIds.Contains(b.ResourceId));
        }

        if (request.UnitOfMeasureIds != null && request.UnitOfMeasureIds.Any())
        {
            query = query.Where(b => request.UnitOfMeasureIds.Contains(b.UnitOfMeasureId));
        }

        var balances = await query.ToListAsync(cancellationToken);
        var resourceIds = balances.Select(b => b.ResourceId).Distinct().ToList();
        var unitIds = balances.Select(b => b.UnitOfMeasureId).Distinct().ToList();

        var resources = await _context.Set<Resource>()
            .Where(r => resourceIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var units = await _context.Set<UnitOfMeasure>()
            .Where(u => unitIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return balances.Select(balance =>
        {
            var resource = resources.First(r => r.Id == balance.ResourceId);
            var unit = units.First(u => u.Id == balance.UnitOfMeasureId);

            return new BalanceDto(
                balance.Id,
                balance.ResourceId,
                resource.Name,
                balance.UnitOfMeasureId,
                unit.Name,
                balance.Quantity.Value
            );
        }).ToList();
    }
}
