using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public sealed class CreateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IReferenceValidationService referenceValidationService,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReceiptCommand, Guid>
{
    public async Task<Guid> Handle(CreateReceiptCommand command, CancellationToken cancellationToken)
    {
        await receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken);
        await referenceValidationService.ValidateResourcesAsync(command.Resources.Select(c => c.ResourceId), cancellationToken);
        await referenceValidationService.ValidateUnitsAsync(command.Resources.Select(c => c.UnitId), cancellationToken);

        var documentId = Guid.NewGuid();
        
        var resources = command.Resources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new {r.ResourceId, r.UnitId})
            .Select(r => ReceiptResource.Create(documentId, r.Key.ResourceId, r.Key.UnitId, r.Sum(c => c.Quantity)))
            .ToList();

        var receiptDocument = ReceiptDocument.Create(command.Number, command.Date, resources);
        receiptRepository.Create(receiptDocument);

        var items = resources.Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity));
        await balanceService.UpdateBalances(items, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}
