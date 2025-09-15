using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class NamedEntityRepository<T>(WarehouseDbContext dbContext) : RepositoryBase<T>(dbContext), INamedEntityRepository<T>
    where T : NamedEntity
{
    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken ctx = default)
    {
        var query = DbContext.Set<T>().AsNoTracking()
            .Where(x => x.Name.ToLower() == name.ToLower() && x.Id != excludeId);

        return await query.AnyAsync(ctx);
    }

    public async Task<List<T>> GetActiveAsync(CancellationToken ctx)
    {
        return await DbContext.Set<T>()
            .Where(x => x.IsActive)
            .ToListAsync(ctx);
    }

    public async Task<List<T>> GetArchivedAsync(CancellationToken ctx)
    {
        return await DbContext.Set<T>()
            .Where(x => !x.IsActive)
            .ToListAsync(ctx);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ctx)
    {
        var entity = await GetByIdAsync(id, ctx);

        entity.Archive();
        Update(entity);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ctx)
    {
        var entity = await GetByIdAsync(id, ctx);

        entity.Activate();
        Update(entity);
    }

    public abstract Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx);
}