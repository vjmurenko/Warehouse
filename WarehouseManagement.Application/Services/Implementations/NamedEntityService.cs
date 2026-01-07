using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Services.Implementations;

public abstract class NamedEntityService<T>(INamedEntityRepository<T> repository, IUnitOfWork unitOfWork, ILogger<NamedEntityService<T>> logger)
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
        logger.LogInformation("Creating entity of type {EntityType} with name: {EntityName}", typeof(T).Name, entity.Name);
        
        if (await Repository.ExistsWithNameAsync(entity.Name, ctx: ctx))
        {
            logger.LogWarning("Duplicate entity detected for type {EntityType} with name: {EntityName}", typeof(T).Name, entity.Name);
            throw new DuplicateEntityException(typeof(T).Name, entity.Name);
        }

        var id = Repository.Create(entity);
        logger.LogInformation("Entity created with temporary ID: {EntityId}", id);

        await unitOfWork.SaveChangesAsync(ctx);
        logger.LogInformation("Entity of type {EntityType} successfully saved with ID: {EntityId}", typeof(T).Name, id);
        return id;
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken ctx)
    {
        logger.LogInformation("Updating entity of type {EntityType} with ID: {EntityId} and name: {EntityName}", typeof(T).Name, entity.Id, entity.Name);
        
        if (await Repository.ExistsWithNameAsync(entity.Name, entity.Id, ctx))
        {
            logger.LogWarning("Duplicate entity detected for type {EntityType} with name: {EntityName} during update", typeof(T).Name, entity.Name);
            throw new DuplicateEntityException(typeof(T).Name, entity.Name);
        }
        
        Repository.Update(entity);
        logger.LogInformation("Entity of type {EntityType} with ID: {EntityId} marked for update", typeof(T).Name, entity.Id);
        
        var result = await unitOfWork.SaveChangesAsync(ctx) > 0;
        logger.LogInformation("Entity update of type {EntityType} with ID: {EntityId} completed successfully: {Success}", typeof(T).Name, entity.Id, result);
        return result;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken ctx)
    {
        logger.LogInformation("Deleting entity of type {EntityType} with ID: {EntityId}", typeof(T).Name, id);
        
        if (await Repository.IsUsingInDocuments(id, ctx))
        {
            logger.LogWarning("Cannot delete entity of type {EntityType} with ID: {EntityId} because it is in use in documents", typeof(T).Name, id);
            throw new EntityInUseException(typeof(T).Name, id, "documents");
        }

        var entity = await Repository.GetByIdAsync(id, ctx);
        if (entity is null)
        {
            logger.LogWarning("Entity of type {EntityType} with ID: {EntityId} not found for deletion", typeof(T).Name, id);
            throw new EntityNotFoundException(typeof(T).Name, id);
        }
        
        Repository.Delete(entity);
        logger.LogInformation("Entity of type {EntityType} with ID: {EntityId} marked for deletion", typeof(T).Name, id);
        
        var result = await unitOfWork.SaveChangesAsync(ctx) > 0;
        logger.LogInformation("Entity deletion of type {EntityType} with ID: {EntityId} completed successfully: {Success}", typeof(T).Name, id, result);
        return result;
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