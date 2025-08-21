using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IBaseRepository<T> where T : Entity
{
    Task<bool> DeleteAsync(T entity);
    Task<T> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<List<T>> GetAll();
}