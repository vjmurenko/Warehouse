using WarehouseManagement.SharedKernel.Business.SharedKernel.Aggregates;

namespace WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

public interface IShipmentRepository : IRepositoryBase<ShipmentDocument>
{
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
}