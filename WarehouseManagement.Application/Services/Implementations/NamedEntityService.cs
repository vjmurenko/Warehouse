using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Services.Implementations;

public abstract class NamedEntityService<T>(INamedEntityRepository<T> repository, IUnitOfWork unitOfWork)
    : INamedEntityService<T>
    where T : NamedEntity
{
    protected readonly INamedEntityRepository<T> Repository = repository;

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ctx)
    {
        return await Repository.GetAllAsync(ctx);
    }

    public virtual async Task<List<T>> GetActiveAsync(CancellationToken ctx)
    {
        return await Repository.GetActiveAsync(ctx);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ctx)
    {
        return await Repository.GetByIdAsync(id, ctx);
    }

    public virtual async Task<Guid> CreateAsync(T entity, CancellationToken ctx)
    {
        if (await Repository.ExistsWithNameAsync(entity.Name, ctx: ctx))
        {
            throw new DuplicateEntityException(typeof(T).Name, entity.Name);
        }

        var id =  Repository.Create(entity);

        await unitOfWork.SaveChangesAsync(ctx);
        return id;
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken ctx)
    {
        if (await Repository.ExistsWithNameAsync(entity.Name, entity.Id, ctx))
        {
            throw new DuplicateEntityException(typeof(T).Name, entity.Name);
        }
        
        Repository.Update(entity);
        return await unitOfWork.SaveChangesAsync(ctx) > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken ctx)
    {
        if (await Repository.IsUsingInDocuments(id, ctx))
        {
            throw new EntityInUseException(typeof(T).Name, id, "documents");
        }

        var entity = await Repository.GetByIdAsync(id, ctx);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(T).Name, id);
        }
        
        Repository.Delete(entity);
        return await unitOfWork.SaveChangesAsync(ctx) > 0;
    }

    public virtual async Task<bool> ArchiveAsync(Guid id, CancellationToken ctx)
    {
         await Repository.ArchiveAsync(id, ctx);
         return await unitOfWork.SaveChangesAsync(ctx) > 0;
    }

    public virtual async Task<bool> ActivateAsync(Guid id, CancellationToken ctx)
    {
        await Repository.ActivateAsync(id, ctx);
        return await unitOfWork.SaveChangesAsync(ctx) > 0;
    }
}