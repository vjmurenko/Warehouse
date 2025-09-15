using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface INamedEntityService<T> where T : NamedEntity
{
    Task<List<T>> GetAllAsync(CancellationToken ctx);
    Task<List<T>> GetActiveAsync(CancellationToken ctx);
    Task<T?> GetByIdAsync(Guid id,CancellationToken ctx);
    Task<Guid> CreateAsync(T entity,CancellationToken ctx);
    Task<bool> UpdateAsync(T entity,CancellationToken ctx);
    Task<bool> DeleteAsync(Guid id,CancellationToken ctx);
    Task<bool> ArchiveAsync(Guid id,CancellationToken ctx);
    Task<bool> ActivateAsync(Guid id,CancellationToken ctx);
}
