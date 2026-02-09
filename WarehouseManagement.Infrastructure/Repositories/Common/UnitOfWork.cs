using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories.Common;

public class UnitOfWork(WarehouseDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
       return await context.SaveChangesAsync(cancellationToken);
    }
    
    public void Dispose()
    {
        context.Dispose();
    }
}