﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public abstract class RepositoryBase<T>(WarehouseDbContext dbContext) : IBaseRepository<T> where T : Entity
{
    public WarehouseDbContext DbContext { get; set; } = dbContext;

    public virtual Task<Guid> CreateAsync(T t)
    {
        DbContext.Add(t);
        return Task.FromResult(t.Id);
    }
    
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await DbContext.Set<T>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<List<T>> GetAll()
    {
        return await DbContext.Set<T>().ToListAsync();
    }

    public Task<bool> UpdateAsync(T t)
    {
        DbContext.Set<T>().Update(t);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(T t)
    {
        DbContext.Remove(t);
        return Task.FromResult(true);
    }

}