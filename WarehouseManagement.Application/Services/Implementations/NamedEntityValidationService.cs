using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Dictionary<Guid, Resource>> ValidateResourcesAsync(IEnumerable<Guid> resourceIds, CancellationToken cancellationToken)
    {
        var distinctIds = resourceIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new Dictionary<Guid, Resource>();

        var resources = await resourceRepository.GetAll();
        var resourcesDict = resources
            .Where(r => distinctIds.Contains(r.Id))
            .ToDictionary(r => r.Id, r => r);

        // Check for missing resources
        var missingIds = distinctIds.Except(resourcesDict.Keys).ToList();
        if (missingIds.Any())
            throw new ArgumentException($"Ресурсы с ID {string.Join(", ", missingIds)} не найдены");

        // Check for inactive resources
        var inactiveResources = resourcesDict.Values.Where(r => !r.IsActive).ToList();
        if (inactiveResources.Any())
        {
            var inactiveNames = string.Join(", ", inactiveResources.Select(r => $"'{r.Name}'"));
            throw new InvalidOperationException($"Ресурсы {inactiveNames} архивированы и не могут быть использованы");
        }

        return resourcesDict;
    }

    public async Task<Dictionary<Guid, UnitOfMeasure>> ValidateUnitsOfMeasureAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken)
    {
        var distinctIds = unitIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new Dictionary<Guid, UnitOfMeasure>();

        var units = await unitRepository.GetAll();
        var unitsDict = units
            .Where(u => distinctIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u);

        // Check for missing units
        var missingIds = distinctIds.Except(unitsDict.Keys).ToList();
        if (missingIds.Any())
            throw new ArgumentException($"Единицы измерения с ID {string.Join(", ", missingIds)} не найдены");

        // Check for inactive units
        var inactiveUnits = unitsDict.Values.Where(u => !u.IsActive).ToList();
        if (inactiveUnits.Any())
        {
            var inactiveNames = string.Join(", ", inactiveUnits.Select(u => $"'{u.Name}'"));
            throw new InvalidOperationException($"Единицы измерения {inactiveNames} архивированы и не могут быть использованы");
        }

        return unitsDict;
    }
}