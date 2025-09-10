using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IShipmentRepository
{
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(ShipmentDocument document, CancellationToken cancellationToken = default);
    Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShipmentDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShipmentDocument document, CancellationToken cancellationToken = default);

}