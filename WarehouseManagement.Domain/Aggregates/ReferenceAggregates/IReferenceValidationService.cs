namespace WarehouseManagement.Domain.Aggregates.ReferenceAggregates;

public interface IReferenceValidationService
{
    Task ValidateResourcesAsync(IEnumerable<Guid> resourceIds, CancellationToken cancellationToken);
    Task ValidateUnitsAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken);
}