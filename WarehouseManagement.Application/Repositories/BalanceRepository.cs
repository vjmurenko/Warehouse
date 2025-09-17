using Microsoft.EntityFrameworkCore;
using Npgsql;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

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
        
        var values = string.Join(", ", keyList.Select((k, i) => $"(@p{i*2}, @p{i*2+1})"));
        var sql = $"""
                       SELECT * 
                       FROM "Balances"
                       WHERE ("ResourceId","UnitOfMeasureId") IN ({values})
                       FOR UPDATE
                   """;

        var parameters = keyList
            .SelectMany((k, i) => new object[]
            {
                new NpgsqlParameter($"p{i*2}", k.ResourceId),
                new NpgsqlParameter($"p{i*2+1}", k.UnitOfMeasureId)
            })
            .ToArray();

        var balancesList = await context.Balances.FromSqlRaw(sql, parameters).ToListAsync(ct);

        var balancesDict = balancesList.ToDictionary(
            b => new ResourceUnitKey(b.ResourceId, b.UnitOfMeasureId),
            b => b);

        return balancesDict;
    }
}