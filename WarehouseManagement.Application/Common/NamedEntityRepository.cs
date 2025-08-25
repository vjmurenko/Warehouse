using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class NamedEntityRepository<T> : RepositoryBase<T>, INamedEntityRepository<T> 
    where T : NamedEntity
{
    public NamedEntityRepository(WarehouseDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        var query = DbContext.Set<T>().AsNoTracking()
            .Where(x => x.Name.ToLower() == name.ToLower());
        
        return await query.AnyAsync();
    }

    public async Task<List<T>> GetActiveAsync()
    {
        return await DbContext.Set<T>()
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<List<T>> GetArchivedAsync()
    {
        return await DbContext.Set<T>()
            .Where(x => !x.IsActive)
            .ToListAsync();
    }

    public async Task<bool> ArchiveAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        entity.Archive();
        return await UpdateAsync(entity);
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        entity.Activate();
        return await UpdateAsync(entity);
    }

    public abstract Task<bool> IsUsingInDocuments(Guid id);
}
