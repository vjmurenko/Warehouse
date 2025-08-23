using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface INamedEntityService<T> where T : NamedEntity
{
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetActiveAsync();
    Task<List<T>> GetArchivedAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ArchiveAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
}
