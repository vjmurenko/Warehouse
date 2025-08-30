using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;

public class GetShipmentsQueryHandler(
    IShipmentRepository shipmentRepository,
    IClientService clientService,
    IUnitOfMeasureService unitOfMeasureService,
    IResourceService resourceService) : IRequestHandler<GetShipmentsQuery, List<ShipmentDocumentDto>>
{
    public async Task<List<ShipmentDocumentDto>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await shipmentRepository.GetFilteredAsync(
            request.FromDate,
            request.ToDate,
            request.DocumentNumbers,
            request.ResourceIds,
            request.UnitIds,
            cancellationToken);

        var result = new List<ShipmentDocumentDto>();

        foreach (var document in documents)
        {
            var client = await clientService.GetByIdAsync(document.ClientId);
            var clientName = client?.Name ?? "Unknown Client";
            var shipmentResourceDetailDtos = new List<ShipmentResourceDetailDto>();

            foreach (var shipmentResource in document.ShipmentResources)
            {
                var unitOfMeasure = await unitOfMeasureService.GetByIdAsync(shipmentResource.UnitOfMeasureId);
                var resource = await resourceService.GetByIdAsync(shipmentResource.ResourceId);
                
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