using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Services.Implementations;

public sealed class ShipmentValidationService(
    INamedEntityRepository<Client> clientRepository,
    INamedEntityValidationService namedEntityValidationService,
    ILogger<ShipmentValidationService> logger) : IShipmentValidationService
{
    public async Task ValidateShipmentResourcesForUpdate(
        List<ShipmentResourceDto> updatedShipmentResources,
        CancellationToken ctx,
        ShipmentDocument? currentDocumentForExclude = null)
    {
        logger.LogInformation("Validating shipment resources for update. Updated resources count: {ResourceCount}", updatedShipmentResources.Count);
        var resourcesForExclude = new List<ShipmentResourceDto>();

        if (currentDocumentForExclude is not null)
        {
            resourcesForExclude.AddRange(currentDocumentForExclude.ShipmentResources
                .Select(c => new ShipmentResourceDto(c.ResourceId, c.UnitOfMeasureId, c.Quantity.Value)));
        }

        var resourcesToValidate = updatedShipmentResources
            .Where(u => !resourcesForExclude.Any(r => r.UnitId == u.UnitId && r.ResourceId == u.ResourceId))
            .ToList();
        
        await namedEntityValidationService.ValidateResourcesAsync(resourcesToValidate.Select(c => c.ResourceId), ctx);
        await namedEntityValidationService.ValidateUnitsAsync(resourcesToValidate.Select(c => c.UnitId), ctx);
        
        logger.LogInformation("Successfully validated shipment resources for update");
    }

    public async Task ValidateClient(Guid clientId, Guid? excludeCurrentClient = null, CancellationToken ctx = default)
    {
        logger.LogInformation("Validating client with ID: {ClientId}", clientId);
        var clients = await clientRepository.GetArchivedAsync(ctx);
        var archivedClient = clients.Where(c => c.Id != excludeCurrentClient).FirstOrDefault(c => c.Id == clientId);

        if (archivedClient is not null)
        {
            logger.LogWarning("Client {ClientId} is archived and cannot be used. Client name: {ClientName}", clientId, archivedClient.Name);
            throw new InvalidOperationException($"Клиент {archivedClient.Name} находится в архиве и не может быть использован");
        }
        logger.LogInformation("Client {ClientId} is valid for use", clientId);
    }
}