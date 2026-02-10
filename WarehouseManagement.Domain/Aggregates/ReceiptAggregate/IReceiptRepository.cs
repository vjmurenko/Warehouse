using WarehouseManagement.SharedKernel.Business.SharedKernel.Aggregates;

namespace WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

public interface IReceiptRepository : IRepositoryBase<ReceiptDocument>
{
    Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
}