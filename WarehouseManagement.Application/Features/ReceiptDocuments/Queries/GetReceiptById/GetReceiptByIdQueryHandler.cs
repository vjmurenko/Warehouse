using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;

public class GetReceiptByIdQueryHandler(
    IReceiptRepository receiptRepository,
    IResourceService resourceService,
    IUnitOfMeasureService unitOfMeasureService) : IRequestHandler<GetReceiptByIdQuery, ReceiptDocumentDto?>
{
    public async Task<ReceiptDocumentDto?> Handle(GetReceiptByIdQuery query, CancellationToken ctx)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(query.Id, ctx);
        
        if (document == null)
            return null;

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

        return new ReceiptDocumentDto(
            document.Id,
            document.Number,
            document.Date,
            resourceDetails
        );
    }
}