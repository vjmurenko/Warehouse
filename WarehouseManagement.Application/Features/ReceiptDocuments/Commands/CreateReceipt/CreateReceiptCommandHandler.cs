using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
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

        var receiptDocument = new ReceiptDocument(command.Number, command.Date);

        foreach (var dto in command.Resources)
        {
            await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
            await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);


            receiptDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
        
        await receiptRepository.AddAsync(receiptDocument, cancellationToken);


        foreach (var resource in receiptDocument.ReceiptResources)
        {
            await balanceService.IncreaseBalance(
                resource.ResourceId,
                resource.UnitOfMeasureId,
                resource.Quantity,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return receiptDocument.Id;
    }
}