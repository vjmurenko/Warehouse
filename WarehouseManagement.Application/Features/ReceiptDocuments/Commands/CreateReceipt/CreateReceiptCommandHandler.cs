using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public sealed class CreateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    INamedEntityValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReceiptCommand, Guid>
{
    public async Task<Guid> Handle(CreateReceiptCommand command, CancellationToken cancellationToken)
    {
        if (await receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        await validationService.ValidateResourcesAsync(command.Resources.Select(c => c.ResourceId), cancellationToken);
        await validationService.ValidateUnitsAsync(command.Resources.Select(c => c.UnitId), cancellationToken);

        var documentId = Guid.NewGuid();
        
        var resources = command.Resources
            .Where(r => r.Quantity > 0)
            .GroupBy(r => new {r.ResourceId, r.UnitId})
            .Select(r => ReceiptResource.Create(documentId, r.Key.ResourceId, r.Key.UnitId, r.Sum(c => c.Quantity)))
            .ToList();

        var receiptDocument = ReceiptDocument.Create(command.Number, command.Date, resources);

        receiptRepository.Create(receiptDocument);

        await unitOfWork.SaveEntitiesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}
