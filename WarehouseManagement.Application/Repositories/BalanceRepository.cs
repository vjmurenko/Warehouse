using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class BalanceRepository(WarehouseDbContext context) : RepositoryBase<Balance>(context), IBalanceRepository
{
    public async Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitId, CancellationToken ct)
    {
        return await DbContext.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitOfMeasureId\" = {1} FOR UPDATE",
                resourceId, unitId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Balance balance, CancellationToken ct)
    {
        await DbContext.Balances.AddAsync(balance, ct);
    }

    public async Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Balances.AsQueryable();

        // Filter out zero balances
        query = query.Where(b => b.Quantity.Value > 0);

        // Resource filtering
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(b => resourceIds.Contains(b.ResourceId));
        }

        // Unit filtering  
        if (unitIds != null && unitIds.Any())
        {
            query = query.Where(b => unitIds.Contains(b.UnitOfMeasureId));
        }

        return await query
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(cancellationToken);
    }
}