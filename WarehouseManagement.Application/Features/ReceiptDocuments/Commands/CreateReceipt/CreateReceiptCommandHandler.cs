using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Adapters;
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
        if (await receiptRepository.ExistsByNumberAsync(command.Number))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");
        
        await validationService.ValidateResourcesAsync(command.Resources.Select(c => c.ResourceId), cancellationToken);
        await validationService.ValidateUnitsAsync(command.Resources.Select(c => c.UnitId), cancellationToken);
        
        var receiptDocument = new ReceiptDocument(command.Number, command.Date);
        foreach (var dto in command.Resources)
        {
            receiptDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
        
        receiptRepository.Create(receiptDocument);
        
        var balancesToIncrease = receiptDocument.ReceiptResources.Select(r => new ReceiptResourceAdapter(r).ToDelta());
        await balanceService.IncreaseBalances(balancesToIncrease, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}