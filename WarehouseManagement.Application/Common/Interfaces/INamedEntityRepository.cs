using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface INamedEntityRepository<T> : IBaseRepository<T> where T : NamedEntity
{
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null);
    Task<List<T>> GetActiveAsync();
    Task<List<T>> GetArchivedAsync();
    Task<bool> ArchiveAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
    Task<bool> IsUsingInDocuments(Guid id);
}
