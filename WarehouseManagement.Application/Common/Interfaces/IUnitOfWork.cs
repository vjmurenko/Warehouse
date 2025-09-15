namespace WarehouseManagement.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken token);
    Task CommitTransactionAsync(CancellationToken token);
    Task RollbackTransactionAsync(CancellationToken token);
    Task<int> SaveChangesAsync(CancellationToken token);
}