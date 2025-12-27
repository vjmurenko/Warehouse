using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement movement, CancellationToken ctx);
    Task AddRangeAsync(IEnumerable<StockMovement> movements, CancellationToken ctx);
    Task<List<StockMovement>> GetByDocumentIdAsync(Guid documentId, CancellationToken ctx);
    Task<decimal> GetBalanceAsync(Guid resourceId, Guid unitId, CancellationToken ctx);
    Task<Dictionary<(Guid ResourceId, Guid UnitId), decimal>> GetBalancesAsync(
        IEnumerable<(Guid ResourceId, Guid UnitId)> keys, CancellationToken ctx);
    Task<List<(Guid ResourceId, Guid UnitId, decimal Quantity)>> GetBalancesFilteredAsync(
        List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken ctx);
}
