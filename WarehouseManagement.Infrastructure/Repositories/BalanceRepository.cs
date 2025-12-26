using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class BalanceRepository(WarehouseDbContext context, ILogger<BalanceRepository> logger) : RepositoryBase<Balance>(context), IBalanceRepository
{
    public async Task AddAsync(Balance balance, CancellationToken ct)
    {
        logger.LogInformation("Adding balance for resource {ResourceId} and unit {UnitOfMeasureId}", balance.ResourceId, balance.UnitOfMeasureId);
        await DbContext.Balances.AddAsync(balance, ct);
        logger.LogInformation("Balance added successfully for resource {ResourceId} and unit {UnitOfMeasureId}", balance.ResourceId, balance.UnitOfMeasureId);
    }

    public async Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting filtered balances with {ResourceCount} resources and {UnitCount} units", resourceIds?.Count ?? 0, unitIds?.Count ?? 0);
        var query = DbContext.Balances.AsQueryable();

        query = query.Where(b => b.Quantity.Value > 0);

        if (resourceIds is not null && resourceIds.Any())
        {
            query = query.Where(b => resourceIds.Contains(b.ResourceId));
        }

        if (unitIds is not null && unitIds.Any())
        {
            query = query.Where(b => unitIds.Contains(b.UnitOfMeasureId));
        }

        var result = await query
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {Count} balances", result.Count);
        return result;
    }
    
    public async Task<Dictionary<ResourceUnitKey, Balance>> GetForUpdateAsync(
        IEnumerable<ResourceUnitKey> keys,
        CancellationToken ct)
    {
        logger.LogInformation("Getting balances for update with {KeyCount} keys", keys.Distinct().Count());
        var keyList = keys.Distinct().ToList();
        if (!keyList.Any())
        {
            logger.LogInformation("No keys provided, returning empty dictionary");
            return new Dictionary<ResourceUnitKey, Balance>();
        }
        
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

        var result = balancesList
            .ToDictionary(b => new ResourceUnitKey(b.ResourceId, b.UnitOfMeasureId), b => b);
        logger.LogInformation("Retrieved {Count} balances for update", result.Count);
        return result;
    }

}