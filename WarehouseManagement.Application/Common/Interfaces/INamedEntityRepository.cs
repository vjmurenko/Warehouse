using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface INamedEntityRepository<T> : IRepositoryBase<T> where T : NamedEntity
{
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken ctx = default);
    Task<List<T>> GetActiveAsync(CancellationToken ctx);
    Task<List<T>> GetArchivedAsync(CancellationToken ctx);
    Task ArchiveAsync(Guid id, CancellationToken ctx);
    Task ActivateAsync(Guid id,CancellationToken ctx);
    Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx);
}
