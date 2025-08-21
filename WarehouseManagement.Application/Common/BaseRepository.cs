using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class RepositoryBase<T>(WarehouseDbContext dbContext) : IBaseRepository<T> where T : Entity
{
    public WarehouseDbContext DbContext { get; set; } = dbContext;

    public virtual async Task<Guid> CreateAsync(T t)
    {
        DbContext.Add(t);
        await Save();
        return t.Id;
    }
    
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await DbContext.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<List<T>> GetAll()
    {
        return await DbContext.Set<T>().ToListAsync();
    }

    public async Task<bool> UpdateAsync(T t)
    {
        DbContext.Set<T>().Update(t);
        return await Save();
    }

    public async Task<bool> DeleteAsync(T t)
    {
        DbContext.Remove(t);
        return await Save();
    }

    public async Task<bool> Save()
    {
        var result = await DbContext.SaveChangesAsync();
        return result > 0;
    }
    
}