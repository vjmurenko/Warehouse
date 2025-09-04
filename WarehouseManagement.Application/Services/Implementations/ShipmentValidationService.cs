using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Services.Implementations;

public class ShipmentValidationService(
    IReceiptRepository receiptRepository,
    INamedEntityRepository<Client> clientRepository,
    INamedEntityValidationService namedEntityValidationService) : IShipmentValidationService
{
    public async Task ValidateShipmentResourcesForUpdate(
        List<ShipmentResourceDto> updatedShipmentResources,
        CancellationToken token,
        ShipmentDocument? currentDocumentForExclude = null)
    {
        var resourcesForExclude = new List<ShipmentResourceDto>();
        var receiptDocuments = await receiptRepository.GetFilteredAsync();
        var receiptResources = receiptDocuments
            .SelectMany(c => c.ReceiptResources, (_, resource) =>
                new ShipmentResourceDto(resource.ResourceId, resource.UnitOfMeasureId, resource.Quantity.Value))
            .ToList();
        resourcesForExclude.AddRange(receiptResources);
        
        if (currentDocumentForExclude != null)
        {
            resourcesForExclude.AddRange(currentDocumentForExclude.ShipmentResources
                .Select(c => new ShipmentResourceDto(c.ResourceId, c.UnitOfMeasureId, c.Quantity.Value)));
        }
        
        updatedShipmentResources = updatedShipmentResources
            .Where(u => 
                !resourcesForExclude.Any(r => r.UnitId == u.UnitId && r.ResourceId == u.ResourceId))
            .ToList();
        
        foreach (var documentReceiptResource in updatedShipmentResources)
        {
            await namedEntityValidationService.ValidateResourceAsync(documentReceiptResource.ResourceId, token);
            await namedEntityValidationService.ValidateUnitOfMeasureAsync(documentReceiptResource.UnitId, token);
        }
    }

    public async Task ValidateClient(Guid clientId, Guid? excludeCurrentClient = null)
    {
        var clients = await clientRepository.GetArchivedAsync();
        var archivedClient = clients.Where(c => c.Id != excludeCurrentClient).FirstOrDefault(c => c.Id == clientId);

        if (archivedClient != null)
        {
            throw new InvalidOperationException($"Клиент {archivedClient.Name} находится в архиве и не может быть использован");
        }
    }
}