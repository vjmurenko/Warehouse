using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IShipmentRepository : IRepositoryBase<ShipmentDocument>
{
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ShipmentDocument>> GetFilteredAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default);
}