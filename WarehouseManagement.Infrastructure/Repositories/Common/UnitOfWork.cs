using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories.Common;

public class UnitOfWork(WarehouseDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken token)
    {
       return await context.SaveChangesAsync(token);
    }
}