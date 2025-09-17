using Microsoft.EntityFrameworkCore;
using Npgsql;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public class BalanceRepository(WarehouseDbContext context) : RepositoryBase<Balance>(context), IBalanceRepository
{
    public async Task AddAsync(Balance balance, CancellationToken ct)
    {
        await DbContext.Balances.AddAsync(balance, ct);
    }

    public async Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Balances.AsQueryable();

        query = query.Where(b => b.Quantity.Value > 0);

        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(b => resourceIds.Contains(b.ResourceId));
        }

        if (unitIds != null && unitIds.Any())
        {
            query = query.Where(b => unitIds.Contains(b.UnitOfMeasureId));
        }

        return await query
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Dictionary<ResourceUnitKey, Balance>> GetForUpdateAsync(
        IEnumerable<ResourceUnitKey> keys,
        CancellationToken ct)
    {
        var keyList = keys.Distinct().ToList();
        if (!keyList.Any())
            return new Dictionary<ResourceUnitKey, Balance>();
        
        var resourceIds = keyList.Select(k => k.ResourceId).ToArray();
        var unitIds = keyList.Select(k => k.UnitOfMeasureId).ToArray();

        var sql = @"
        SELECT *
        FROM ""Balances""
        WHERE (""ResourceId"", ""UnitOfMeasureId"") IN (
            SELECT UNNEST(@resourceIds::uuid[]), UNNEST(@unitIds::uuid[])
        )
        FOR UPDATE";

        var parameters = new[]
        {
            new NpgsqlParameter("resourceIds", resourceIds),
            new NpgsqlParameter("unitIds", unitIds)
        };

        var balancesList = await context.Balances
            .FromSqlRaw(sql, parameters)
            .ToListAsync(ct);

        return balancesList
            .ToDictionary(b => new ResourceUnitKey(b.ResourceId, b.UnitOfMeasureId), b => b);
    }

}