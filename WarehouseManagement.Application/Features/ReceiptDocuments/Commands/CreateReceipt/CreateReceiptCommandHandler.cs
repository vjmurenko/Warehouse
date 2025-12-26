using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;

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
        
        var receiptDocument = new ReceiptDocument(command.Number, command.Date);
        
        receiptDocument.SetResources(command.Resources.Select(c => new BalanceDelta(c.ResourceId, c.UnitId, c.Quantity)));
        
        receiptRepository.Create(receiptDocument);
        
        await unitOfWork.SaveEntitiesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}