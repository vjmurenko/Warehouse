using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IBalanceRepository : IRepositoryBase<Balance>
{
    Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitOfMeasureId, CancellationToken token);
    Task AddAsync(Balance balance, CancellationToken token);
    Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken cancellationToken = default);
}