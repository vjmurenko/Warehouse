using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates.BalanceAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class BalanceRepository(WarehouseDbContext context) : RepositoryBase<Balance>(context), IBalanceRepository
{
    public async Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitOfMeasureId, CancellationToken ctx)
    {
        return await context.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitOfMeasureId\" = {1} FOR UPDATE", resourceId, unitOfMeasureId)
            .SingleOrDefaultAsync(ctx);
    }

    public async Task<List<Balance>> GetForUpdateAsync(IEnumerable<(Guid ResourceId, Guid UnitId)> keys, CancellationToken ctx)
    {
        var keyList = keys.ToList();
        if (keyList.Count == 0) return new List<Balance>();

        var balances = new List<Balance>();
        foreach (var (resourceId, unitId) in keyList)
        {
            var balance = await context.Balances
                .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitOfMeasureId\" = {1} FOR UPDATE", resourceId, unitId)
                .SingleOrDefaultAsync(ctx);
            
            if (balance != null)
                balances.Add(balance);
        }

        return balances;
    }

    public async Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken ctx)
    {
        var query = context.Balances.AsQueryable();

        if (resourceIds is not null && resourceIds.Count > 0)
            query = query.Where(b => resourceIds.Contains(b.ResourceId));

        if (unitIds is not null && unitIds.Count > 0)
            query = query.Where(b => unitIds.Contains(b.UnitOfMeasureId));

        return await query
            .Where(b => b.Quantity > 0)
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(ctx);
    }
}
