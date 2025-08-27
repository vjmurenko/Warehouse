using Microsoft.EntityFrameworkCore.Storage;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Common;

public class UnitOfWork(WarehouseDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);   // << SaveChanges только тут
        if (_transaction != null)
            await _transaction.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            await _transaction.RollbackAsync(ct);
    }
}