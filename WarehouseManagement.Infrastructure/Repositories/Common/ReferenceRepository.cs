using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories.Common;

public abstract class ReferenceRepository<T>(WarehouseDbContext dbContext) : RepositoryBase<T>(dbContext), IReferenceRepository<T>
    where T : Reference
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
    
    public abstract Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx);
}