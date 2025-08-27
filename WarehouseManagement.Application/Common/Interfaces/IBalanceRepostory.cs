using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IBalanceRepository
{
    Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitOfMeasureId, CancellationToken token);
    Task AddAsync(Balance balance, CancellationToken token);
}