using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;

public class GetShipmentsQueryHandler(
    IShipmentRepository shipmentRepository,
    INamedEntityRepository<Client> clientRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository,
    INamedEntityRepository<Resource> resourceRepository) : IRequestHandler<GetShipmentsQuery, List<ShipmentDocumentDto>>
{
    public async Task<List<ShipmentDocumentDto>> Handle(GetShipmentsQuery request, CancellationToken ctx)
    {
        var documents = await shipmentRepository.GetFilteredAsync(
            request.FromDate,
            request.ToDate,
            request.DocumentNumbers,
            request.ResourceIds,
            request.UnitIds,
            request.ClientIds,
            ctx);

        var result = new List<ShipmentDocumentDto>();

        var clientIds = documents.Select(d => d.ClientId).Distinct().ToList();
        var resourceIds = documents.SelectMany(d => d.ShipmentResources.Select(r => r.ResourceId)).Distinct();
        var unitIds = documents.SelectMany(d => d.ShipmentResources.Select(r => r.UnitOfMeasureId)).Distinct();
        
        var clients = (await clientRepository.GetByIdsAsync(clientIds, ctx)).ToList();
        var resources = (await resourceRepository.GetByIdsAsync(resourceIds, ctx)).ToList();
        var units = (await unitOfMeasureRepository.GetByIdsAsync(unitIds, ctx)).ToList();

        foreach (var document in documents)
        {
            var client = clients.FirstOrDefault(c => c.Id == document.ClientId);
            var clientName = client?.Name ?? "Unknown Client";
            var shipmentResourceDetailDtos = new List<ShipmentResourceDetailDto>();

            foreach (var shipmentResource in document.ShipmentResources)
            {
                var unitOfMeasure = units.FirstOrDefault(u => u.Id == shipmentResource.UnitOfMeasureId);
                var resource = resources.FirstOrDefault(r => r.Id == shipmentResource.ResourceId);
                
                if (resource != null && unitOfMeasure != null)
                {
                    shipmentResourceDetailDtos.Add(new ShipmentResourceDetailDto(shipmentResource.Id,
                        resource.Id,
                        resource.Name,
                        unitOfMeasure.Id,
                        unitOfMeasure.Name,
                        shipmentResource.Quantity.Value));
                }
            }

            result.Add(new ShipmentDocumentDto(
                document.Id,
                document.Number,
                document.ClientId,
                clientName,
                document.Date,
                document.IsSigned,
                shipmentResourceDetailDtos
                ));
        }

        return result;
    }
}