using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReceiptCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Получение документа для удаления
            var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

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
            await receiptRepository.DeleteAsync(document, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Unit.Value;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}