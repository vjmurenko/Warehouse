using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IReceiptRepository : IRepositoryBase<ReceiptDocument>
{
    Task<bool> ExistsByNumberAsync(string number);
    
    Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<List<ReceiptDocument>> GetFilteredAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        List<string>? documentNumbers = null, 
        List<Guid>? resourceIds = null, 
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default);
}