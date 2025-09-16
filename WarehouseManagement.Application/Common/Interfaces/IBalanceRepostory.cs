using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IBalanceRepository : IRepositoryBase<Balance>
{
    Task<Dictionary<ResourceKey, Balance>> GetForUpdateAsync(IEnumerable<ResourceKey> keys, CancellationToken ct);
    Task AddAsync(Balance balance, CancellationToken token);
    Task<List<Balance>> GetFilteredAsync(List<Guid>? resourceIds, List<Guid>? unitIds, CancellationToken cancellationToken = default);
}