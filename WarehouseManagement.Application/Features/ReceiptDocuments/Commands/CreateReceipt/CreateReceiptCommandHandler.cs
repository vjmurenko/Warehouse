using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public class CreateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
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
        
        var balanceDeltas  = command.Resources
            .GroupBy(r => new { r.ResourceId, r.UnitId })
            .Select(c => new BalanceDelta(c.Key.ResourceId, c.Key.UnitId, c.Sum(r => r.Quantity)))
            .ToList();

        foreach (var balanceDelta in balanceDeltas)
        {
            receiptDocument.AddResource(balanceDelta.ResourceId, balanceDelta.UnitOfMeasureId, balanceDelta.Quantity);
        }
        
        receiptRepository.Create(receiptDocument);
        
        await balanceService.IncreaseBalances(balanceDeltas, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}