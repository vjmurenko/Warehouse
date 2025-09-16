using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Services.Implementations;

public class ShipmentValidationService(
    IReceiptRepository receiptRepository,
    INamedEntityRepository<Client> clientRepository,
    INamedEntityValidationService namedEntityValidationService) : IShipmentValidationService
{
    public async Task ValidateShipmentResourcesForUpdate(
        List<ShipmentResourceDto> updatedShipmentResources,
        CancellationToken ctx,
        ShipmentDocument? currentDocumentForExclude = null)
    {
        var resourcesForExclude = new List<ShipmentResourceDto>();

        if (currentDocumentForExclude != null)
        {
            resourcesForExclude.AddRange(currentDocumentForExclude.ShipmentResources
                .Select(c => new ShipmentResourceDto(c.ResourceId, c.UnitOfMeasureId, c.Quantity.Value)));
        }

        var resourcesToValidate = updatedShipmentResources
            .Where(u => !resourcesForExclude.Any(r => r.UnitId == u.UnitId && r.ResourceId == u.ResourceId))
            .ToList();

        await namedEntityValidationService.ValidateResourcesAsync(resourcesToValidate.Select(c => c.ResourceId), ctx);
        await namedEntityValidationService.ValidateUnitsAsync(resourcesToValidate.Select(c => c.UnitId), ctx);
    }

    public async Task ValidateClient(Guid clientId, Guid? excludeCurrentClient = null, CancellationToken ctx = default)
    {
        var clients = await clientRepository.GetArchivedAsync(ctx);
        var archivedClient = clients.Where(c => c.Id != excludeCurrentClient).FirstOrDefault(c => c.Id == clientId);

        if (archivedClient != null)
        {
            throw new InvalidOperationException($"Клиент {archivedClient.Name} находится в архиве и не может быть использован");
        }
    }
}