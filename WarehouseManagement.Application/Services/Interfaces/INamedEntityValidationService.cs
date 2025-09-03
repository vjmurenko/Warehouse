using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface INamedEntityValidationService
{
    Task<Resource> ValidateResourceAsync(Guid resourceId, CancellationToken cancellationToken);
    Task<UnitOfMeasure> ValidateUnitOfMeasureAsync(Guid unitId, CancellationToken cancellationToken);
}