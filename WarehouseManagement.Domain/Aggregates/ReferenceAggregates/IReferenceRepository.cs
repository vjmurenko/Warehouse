using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Business.SharedKernel.Aggregates;

namespace WarehouseManagement.Domain.Aggregates.ReferenceAggregates;

public interface IReferenceRepository<T> : IRepositoryBase<T> where T : Reference
{
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken ctx = default);
    Task<List<T>> GetActiveAsync(CancellationToken ctx);
    Task<List<T>> GetArchivedAsync(CancellationToken ctx);
    Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx);
}
