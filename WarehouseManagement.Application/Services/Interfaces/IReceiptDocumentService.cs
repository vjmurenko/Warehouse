using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IReceiptDocumentService
{
    Task AddResourceToReceiptAsync(
        ReceiptDocument receiptDocument,
        Guid resourceId,
        Guid unitId,
        decimal quantity,
        CancellationToken ctx);
}