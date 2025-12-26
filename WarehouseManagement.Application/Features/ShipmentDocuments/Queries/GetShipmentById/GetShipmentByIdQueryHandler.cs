using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;

public class GetShipmentByIdQueryHandler(
    IShipmentRepository shipmentRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository,
    IClientService clientService) : IRequestHandler<GetShipmentByIdQuery, ShipmentDocumentDto?>
{
    public async Task<ShipmentDocumentDto?> Handle(GetShipmentByIdQuery request, CancellationToken ctx)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(request.Id, ctx);
        if (document is null)
            return null;

        var client = await clientService.GetByIdAsync(document.ClientId, ctx);
        var resourceIds = document.ShipmentResources.Select(r => r.ResourceId).Distinct();
        var unitIds = document.ShipmentResources.Select(r => r.UnitOfMeasureId).Distinct();
        
        var resources = (await resourceRepository.GetByIdsAsync(resourceIds, ctx)).ToList();
        var units = (await unitOfMeasureRepository.GetByIdsAsync(unitIds, ctx)).ToList();
        
        var resourceDetails = new List<ShipmentResourceDetailDto>();
        foreach (var resource in document.ShipmentResources)
        {
            var resourceInfo = resources.FirstOrDefault(r => r.Id == resource.ResourceId);
            var unitInfo = units.FirstOrDefault(u => u.Id == resource.UnitOfMeasureId);

            resourceDetails.Add(new ShipmentResourceDetailDto(
                resource.Id,
                resource.ResourceId,
                resourceInfo?.Name ?? "Unknown Resource",
                resource.UnitOfMeasureId,
                unitInfo?.Name ?? "Unknown Unit",
                resource.Quantity.Value));
        }

        return new ShipmentDocumentDto(
            document.Id,
            document.Number,
            document.ClientId,
            client?.Name ?? "Unknown Client",
            document.Date,
            document.IsSigned,
            resourceDetails);
    }
}