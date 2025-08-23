using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Services.Implementations;

public abstract class NamedEntityService<T> : INamedEntityService<T> where T : NamedEntity
{
    protected readonly INamedEntityRepository<T> Repository;

    protected NamedEntityService(INamedEntityRepository<T> repository)
    {
        Repository = repository;
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await Repository.GetAll();
    }

    public virtual async Task<List<T>> GetActiveAsync()
    {
        return await Repository.GetActiveAsync();
    }

    public virtual async Task<List<T>> GetArchivedAsync()
    {
        return await Repository.GetArchivedAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await Repository.GetByIdAsync(id);
    }
    
    public virtual async Task<Guid> CreateAsync(T entity)
    {
        if (await Repository.ExistsWithNameAsync(entity.Name))
        {
            throw new InvalidOperationException($"Entity with name '{entity.Name}' already exists.");
        }

        return await Repository.CreateAsync(entity);
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        if (await Repository.ExistsWithNameAsync(entity.Name, entity.Id))
        {
            throw new InvalidOperationException($"Entity with name '{entity.Name}' already exists.");
        }

        return await Repository.UpdateAsync(entity);
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        if (!await Repository.IsUsingInDocuments(id))
        {
            throw new InvalidOperationException("Cannot delete entity - it is used in documents.");
        }

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return false;

        return await Repository.DeleteAsync(entity);
    }

    public virtual async Task<bool> ArchiveAsync(Guid id)
    {
        return await Repository.ArchiveAsync(id);
    }

    public virtual async Task<bool> ActivateAsync(Guid id)
    {
        return await Repository.ActivateAsync(id);
    }
}
