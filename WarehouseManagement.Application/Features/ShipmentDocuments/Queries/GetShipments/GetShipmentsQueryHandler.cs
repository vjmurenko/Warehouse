using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;

public class GetShipmentsQueryHandler(
    IShipmentRepository shipmentRepository,
    IClientService clientService) : IRequestHandler<GetShipmentsQuery, List<ShipmentDocumentSummaryDto>>
{
    public async Task<List<ShipmentDocumentSummaryDto>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await shipmentRepository.GetFilteredAsync(
            request.FromDate,
            request.ToDate,
            request.DocumentNumbers,
            request.ResourceIds,
            request.UnitIds,
            cancellationToken);

        var result = new List<ShipmentDocumentSummaryDto>();
        
        foreach (var document in documents)
        {
            var client = await clientService.GetByIdAsync(document.ClientId);
            var clientName = client?.Name ?? "Unknown Client";

            result.Add(new ShipmentDocumentSummaryDto(
                document.Id,
                document.Number,
                document.ClientId,
                clientName,
                document.Date,
                document.IsSigned,
                document.ShipmentResources.Count));
        }

        return result;
    }
}