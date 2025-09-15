using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;

public class GetReceiptsQueryHandler(
    IReceiptRepository receiptRepository,
    IResourceService resourceService,
    IUnitOfMeasureService unitOfMeasureService) : IRequestHandler<GetReceiptsQuery, List<ReceiptDocumentDto>>
{
    public async Task<List<ReceiptDocumentDto>> Handle(GetReceiptsQuery query, CancellationToken ctx)
    {
        var documents = await receiptRepository.GetFilteredAsync(
            query.FromDate,
            query.ToDate,
            query.DocumentNumbers,
            query.ResourceIds,
            query.UnitIds,
            ctx);

        var result = new List<ReceiptDocumentDto>();
        
        foreach (var document in documents)
        {
            var resourceDetails = new List<ReceiptResourceDetailDto>();
            
            foreach (var resource in document.ReceiptResources)
            {
                var resourceEntity = await resourceService.GetByIdAsync(resource.ResourceId, ctx);
                var unitEntity = await unitOfMeasureService.GetByIdAsync(resource.UnitOfMeasureId, ctx);
                
                if (resourceEntity != null && unitEntity != null)
                {
                    resourceDetails.Add(new ReceiptResourceDetailDto(
                        resource.Id,
                        resource.ResourceId,
                        resourceEntity.Name,
                        resource.UnitOfMeasureId,
                        unitEntity.Name,
                        resource.Quantity.Value
                    ));
                }
            }
            
            result.Add(new ReceiptDocumentDto(
                document.Id,
                document.Number,
                document.Date,
                resourceDetails
            ));
        }
        
        return result;
    }
}