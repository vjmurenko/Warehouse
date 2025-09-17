using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class RepositoryBase<T>(WarehouseDbContext dbContext) : IRepositoryBase<T> where T : Entity
{
    protected WarehouseDbContext DbContext { get; set; } = dbContext;

    public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ctx)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await DbContext.Set<T>()
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(ctx);
    }

    public virtual Guid Create(T t)
    {
        DbContext.Add(t);
        return t.Id;
    }
    
    public async Task<T> GetByIdAsync(Guid id, CancellationToken ctx)
    {
        return await DbContext.Set<T>().FirstOrDefaultAsync(c => c.Id == id, ctx);
    }
    
    public async Task<List<T>> GetAllAsync(CancellationToken ctx)
    {
        return await DbContext.Set<T>().ToListAsync(ctx);
    }

    public void  Update(T t)
    {
        DbContext.Set<T>().Update(t);
    }

    public void Delete(T t)
    {
        DbContext.Remove(t);
    }
}