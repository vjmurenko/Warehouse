using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface INamedEntityValidationService
{
    Task ValidateResourcesAsync(IEnumerable<Guid> resourceIds, CancellationToken cancellationToken);
    Task ValidateUnitsAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken);
}