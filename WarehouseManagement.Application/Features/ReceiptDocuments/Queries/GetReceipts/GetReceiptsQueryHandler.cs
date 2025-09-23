using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;

public class GetReceiptsQueryHandler(
    IReceiptRepository receiptRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository) : IRequestHandler<GetReceiptsQuery, List<ReceiptDocumentDto>>
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
        
        var resourceIds = documents.SelectMany(d => d.ReceiptResources.Select(r => r.ResourceId)).Distinct();
        var unitIds = documents.SelectMany(d => d.ReceiptResources.Select(r => r.UnitOfMeasureId)).Distinct();
        
        var resources = (await resourceRepository.GetByIdsAsync(resourceIds, ctx)).ToList();
        var units = (await unitOfMeasureRepository.GetByIdsAsync(unitIds, ctx)).ToList();
        
        foreach (var document in documents)
        {
            var resourceDetails = new List<ReceiptResourceDetailDto>();
            
            foreach (var resource in document.ReceiptResources)
            {
                var resourceEntity = resources.FirstOrDefault(r => r.Id == resource.ResourceId);
                var unitEntity = units.FirstOrDefault(u => u.Id == resource.UnitOfMeasureId);
                
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