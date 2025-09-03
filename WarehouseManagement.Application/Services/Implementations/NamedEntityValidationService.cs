using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class NamedEntityValidationService(
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitRepository) : INamedEntityValidationService
{
    public async Task<Resource> ValidateResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource == null)
            throw new ArgumentException($"Ресурс с ID {resourceId} не найден", nameof(resourceId));
        
        if (!resource.IsActive)
            throw new InvalidOperationException($"Ресурс '{resource.Name}' архивирован и не может быть использован");
            
        return resource;
    }

    public async Task<UnitOfMeasure> ValidateUnitOfMeasureAsync(Guid unitId, CancellationToken cancellationToken)
    {
        var unit = await unitRepository.GetByIdAsync(unitId);
        if (unit == null)
            throw new ArgumentException($"Единица измерения с ID {unitId} не найдена", nameof(unitId));
        
        if (!unit.IsActive)
            throw new InvalidOperationException($"Единица измерения '{unit.Name}' архивирована и не может быть использована");
            
        return unit;
    }
}