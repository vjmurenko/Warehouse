using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReceiptCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
     
            // 1. Получение документа для удаления
            var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new EntityNotFoundException("ReceiptDocument", command.Id);

            // 2. Откат изменений баланса (уменьшение баланса на количество поступивших ресурсов)
            // Валидация возможности удаления происходит внутри DecreaseBalance
            foreach (var resource in document.ReceiptResources)
            {
                await balanceService.DecreaseBalance(
                    resource.ResourceId,
                    resource.UnitOfMeasureId,
                    resource.Quantity,
                    cancellationToken);
            }

            // 3. Удаление документа
            receiptRepository.Delete(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        
    }
}