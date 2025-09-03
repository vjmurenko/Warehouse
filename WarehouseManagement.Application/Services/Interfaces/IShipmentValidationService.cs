using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IShipmentValidationService
{
    Task ValidateShipmentResourcesForUpdate(
        List<ShipmentResourceDto> updatedShipmentResources,
        CancellationToken token,
        ShipmentDocument? currentDocumentForExclude = null);

    Task ValidateClient(Guid clientId, Guid? excludeCurrentClient = null);
}