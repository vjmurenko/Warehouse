namespace WarehouseManagement.SharedKernel.Business.SharedKernel.Aggregates;

public interface IRepositoryBase<T> where T : Entity<Guid>
{
    void Delete(T entity);
    Task<T> GetByIdAsync(Guid id, CancellationToken ctx);
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ctx);
    Guid Create(T entity);
    void Update(T entity);
    Task<List<T>> GetAllAsync(CancellationToken ctx);
}
