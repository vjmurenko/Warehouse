using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;

public class GetReceiptByIdQueryHandler(
    IReceiptRepository receiptRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitOfMeasureRepository) : IRequestHandler<GetReceiptByIdQuery, ReceiptDocumentDto?>
{
    public async Task<ReceiptDocumentDto?> Handle(GetReceiptByIdQuery query, CancellationToken ctx)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(query.Id, ctx);
        
        if (document == null)
            return null;

        var resourceDetails = new List<ReceiptResourceDetailDto>();
        
        var resourceIds = document.ReceiptResources.Select(r => r.ResourceId).Distinct();
        var unitIds = document.ReceiptResources.Select(r => r.UnitOfMeasureId).Distinct();
        
        var resources = (await resourceRepository.GetByIdsAsync(resourceIds, ctx)).ToList();
        var units = (await unitOfMeasureRepository.GetByIdsAsync(unitIds, ctx)).ToList();
        
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

        return new ReceiptDocumentDto(
            document.Id,
            document.Number,
            document.Date,
            resourceDetails
        );
    }
}