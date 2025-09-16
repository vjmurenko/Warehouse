using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class NamedEntityValidationService(
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitRepository)
    : INamedEntityValidationService
{
    
    public async Task ValidateResourcesAsync(IEnumerable<Guid> resourceIds, CancellationToken cancellationToken)
    {
        var ids = resourceIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var resources = await resourceRepository.GetByIdsAsync(ids, cancellationToken);

        // Проверяем, что все ресурсы найдены
        var missingIds = ids.Except(resources.Select(r => r.Id)).ToList();
        if (missingIds.Count > 0)
            throw new ArgumentException($"Не найдены ресурсы с ID: {string.Join(", ", missingIds)}");

        // Проверяем на архив
        var archived = resources.Where(r => !r.IsActive).ToList();
        if (archived.Count > 0)
            throw new InvalidOperationException(
                $"Следующие ресурсы архивированы и не могут быть использованы: {string.Join(", ", archived.Select(r => r.Name))}");
    }

    public async Task ValidateUnitsAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken)
    {
        var ids = unitIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var units = await unitRepository.GetByIdsAsync(ids, cancellationToken);

        var missingIds = ids.Except(units.Select(u => u.Id)).ToList();
        if (missingIds.Count > 0)
            throw new ArgumentException($"Не найдены единицы измерения с ID: {string.Join(", ", missingIds)}");

        var archived = units.Where(u => !u.IsActive).ToList();
        if (archived.Count > 0)
            throw new InvalidOperationException(
                $"Следующие единицы измерения архивированы и не могут быть использованы: {string.Join(", ", archived.Select(u => u.Name))}");
    }
}