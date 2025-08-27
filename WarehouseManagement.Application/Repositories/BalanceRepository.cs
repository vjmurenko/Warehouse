using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class BalanceRepository(WarehouseDbContext context) : IBalanceRepository
{
    public async Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitId, CancellationToken ct)
    {
        return await context.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitId\" = {1} FOR UPDATE",
                resourceId, unitId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Balance balance, CancellationToken ct)
    {
        await context.Balances.AddAsync(balance, ct);
    }
}