using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Services.Implementations;

public class ReceiptDocumentService(INamedEntityRepository<Resource> resourceRepository, INamedEntityRepository<UnitOfMeasure> unitRepository) : IReceiptDocumentService
{
    public async Task AddResourceToReceiptAsync(
        ReceiptDocument receiptDocument,
        Guid resourceId,
        Guid unitId,
        decimal quantity,
        CancellationToken ct)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource == null || !resource.IsActive)
            throw new Exception($"Ресурс {resourceId} не найден или архивирован");

        var unit = await unitRepository.GetByIdAsync(unitId);
        if (unit == null || !unit.IsActive)
            throw new Exception($"Единица измерения {unitId} не найдена или архивирована");

        receiptDocument.AddResource(resource.Id, unit.Id, quantity);
    }
}