using WarehouseManagement.SharedKernel.Business.SharedKernel.Aggregates;

namespace WarehouseManagement.Domain.Aggregates.BalanceAggregate;

public interface IBalanceRepository : IRepositoryBase<Balance>
{
    Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitOfMeasureId, CancellationToken ctx);
    Task<List<Balance>> GetForUpdateAsync(IEnumerable<(Guid ResourceId, Guid UnitId)> keys, CancellationToken ctx);
    Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken ctx);
}
