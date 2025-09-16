using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IRepositoryBase<T> where T : Entity
{
    void Delete(T entity);
    Task<T> GetByIdAsync(Guid id, CancellationToken ctx);
    Guid Create(T entity);
    void Update(T entity);
    Task<List<T>> GetAllAsync(CancellationToken ctx);
}