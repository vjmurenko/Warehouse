using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IReceiptRepository
{
    Task<bool> ExistsByNumberAsync(string number);
    Task AddAsync(ReceiptDocument document, CancellationToken token);
}