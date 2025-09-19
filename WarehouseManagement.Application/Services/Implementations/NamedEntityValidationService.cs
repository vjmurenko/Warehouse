using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class NamedEntityValidationService(
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitRepository,
    ILogger<NamedEntityValidationService> logger)
    : INamedEntityValidationService
{
    public async Task ValidateResourcesAsync(IEnumerable<Guid> resourceIds, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating {ResourceCount} resources", resourceIds.Count());
        
        var ids = resourceIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            logger.LogInformation("No resources to validate, returning early");
            return;
        }

        var resources = await resourceRepository.GetByIdsAsync(ids, cancellationToken);

        // Проверяем, что все ресурсы найдены
        var missingIds = ids.Except(resources.Select(r => r.Id)).ToList();
        if (missingIds.Count > 0)
        {
            logger.LogWarning("Missing resources with IDs: {MissingIds}", string.Join(", ", missingIds));
            throw new ArgumentException($"Не найдены ресурсы с ID: {string.Join(", ", missingIds)}");
        }

        // Проверяем на архив
        var archived = resources.Where(r => !r.IsActive).ToList();
        if (archived.Count > 0)
        {
            logger.LogWarning("Archived resources found: {ArchivedResources}", string.Join(", ", archived.Select(r => r.Name)));
            throw new InvalidOperationException(
                $"Следующие ресурсы архивированы и не могут быть использованы: {string.Join(", ", archived.Select(r => r.Name))}");
        }
        logger.LogInformation("Successfully validated {ResourceCount} resources", resourceIds.Count());
    }

    public async Task ValidateUnitsAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating {UnitCount} units of measure", unitIds.Count());
        var ids = unitIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            logger.LogInformation("No units of measure to validate, returning early");
            return;
        }
        
        var units = await unitRepository.GetByIdsAsync(ids, cancellationToken);

        var missingIds = ids.Except(units.Select(u => u.Id)).ToList();
        if (missingIds.Count > 0)
        {
            logger.LogWarning("Missing units of measure with IDs: {MissingIds}", string.Join(", ", missingIds));
            throw new ArgumentException($"Не найдены единицы измерения с ID: {string.Join(", ", missingIds)}");
        }

        var archived = units.Where(u => !u.IsActive).ToList();
        if (archived.Count > 0)
        {
            logger.LogWarning("Archived units of measure found: {ArchivedUnits}", string.Join(", ", archived.Select(u => u.Name)));
            throw new InvalidOperationException(
                $"Следующие единицы измерения архивированы и не могут быть использованы: {string.Join(", ", archived.Select(u => u.Name))}");
        }
        logger.LogInformation("Successfully validated {UnitCount} units of measure", unitIds.Count());
    }
}