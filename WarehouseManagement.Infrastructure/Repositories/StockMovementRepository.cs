using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class StockMovementRepository(WarehouseDbContext context) : IStockMovementRepository
{
    public async Task AddAsync(StockMovement movement, CancellationToken ctx)
    {
        await context.StockMovements.AddAsync(movement, ctx);
    }

    public async Task AddRangeAsync(IEnumerable<StockMovement> movements, CancellationToken ctx)
    {
        await context.StockMovements.AddRangeAsync(movements, ctx);
    }

    public async Task<List<StockMovement>> GetByDocumentIdAsync(Guid documentId, CancellationToken ctx)
    {
        return await context.StockMovements
            .Where(m => m.DocumentId == documentId)
            .ToListAsync(ctx);
    }

    public async Task<decimal> GetBalanceAsync(Guid resourceId, Guid unitId, CancellationToken ctx)
    {
        return await context.StockMovements
            .Where(m => m.ResourceId == resourceId && m.UnitOfMeasureId == unitId)
            .SumAsync(m => m.Quantity, ctx);
    }

    public async Task<Dictionary<(Guid ResourceId, Guid UnitId), decimal>> GetBalancesAsync(
        IEnumerable<(Guid ResourceId, Guid UnitId)> keys,
        CancellationToken ctx)
    {
        var keyList = keys.ToList();
        if (keyList.Count == 0)
            return new Dictionary<(Guid, Guid), decimal>();

        var resourceIds = keyList.Select(k => k.ResourceId).Distinct().ToList();
        var unitIds = keyList.Select(k => k.UnitId).Distinct().ToList();

        var balances = await context.StockMovements
            .Where(m => resourceIds.Contains(m.ResourceId) && unitIds.Contains(m.UnitOfMeasureId))
            .GroupBy(m => new { m.ResourceId, m.UnitOfMeasureId })
            .Select(g => new { g.Key.ResourceId, g.Key.UnitOfMeasureId, Total = g.Sum(m => m.Quantity) })
            .ToListAsync(ctx);

        return balances
            .Where(b => keyList.Contains((b.ResourceId, b.UnitOfMeasureId)))
            .ToDictionary(b => (b.ResourceId, b.UnitOfMeasureId), b => b.Total);
    }

    public async Task<List<(Guid ResourceId, Guid UnitId, decimal Quantity)>> GetBalancesFilteredAsync(
        List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken ctx)
    {
        var query = context.StockMovements.AsQueryable();

        if (resourceIds is not null && resourceIds.Count > 0)
            query = query.Where(m => resourceIds.Contains(m.ResourceId));

        if (unitIds is not null && unitIds.Count > 0)
            query = query.Where(m => unitIds.Contains(m.UnitOfMeasureId));

        var balances = await query
            .GroupBy(m => new { m.ResourceId, m.UnitOfMeasureId })
            .Select(g => new { g.Key.ResourceId, g.Key.UnitOfMeasureId, Total = g.Sum(m => m.Quantity) })
            .Where(b => b.Total > 0)
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(ctx);

        return balances.Select(b => (b.ResourceId, b.UnitOfMeasureId, b.Total)).ToList();
    }
}
