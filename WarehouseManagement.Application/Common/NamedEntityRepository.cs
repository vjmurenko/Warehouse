using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class NamedEntityRepository<T>(WarehouseDbContext dbContext) : RepositoryBase<T>(dbContext), INamedEntityRepository<T>
    where T : NamedEntity
{
    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null)
    {
        var query = DbContext.Set<T>().AsNoTracking()
            .Where(x => x.Name.ToLower() == name.ToLower() && x.Id != excludeId);
        
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
