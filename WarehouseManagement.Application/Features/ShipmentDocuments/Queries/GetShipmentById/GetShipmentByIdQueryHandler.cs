using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;

public class GetShipmentByIdQueryHandler(
    IShipmentRepository shipmentRepository,
    IResourceService resourceService,
    IUnitOfMeasureService unitOfMeasureService,
    IClientService clientService) : IRequestHandler<GetShipmentByIdQuery, ShipmentDocumentDto?>
{
    public async Task<ShipmentDocumentDto?> Handle(GetShipmentByIdQuery request, CancellationToken ctx)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(request.Id, ctx);
        if (document == null)
            return null;

        // Получаем информацию о клиенте
        var client = await clientService.GetByIdAsync(document.ClientId, ctx);
        var clientName = client?.Name ?? "Unknown Client";

        // Получаем детали ресурсов
        var resourceDetails = new List<ShipmentResourceDetailDto>();
        foreach (var resource in document.ShipmentResources)
        {
            var resourceInfo = await resourceService.GetByIdAsync(resource.ResourceId, ctx);
            var unitInfo = await unitOfMeasureService.GetByIdAsync(resource.UnitOfMeasureId, ctx);

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
            clientName,
            document.Date,
            document.IsSigned,
            resourceDetails);
    }
}