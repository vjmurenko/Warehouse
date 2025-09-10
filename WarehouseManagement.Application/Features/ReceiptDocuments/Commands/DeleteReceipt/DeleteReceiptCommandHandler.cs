using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IReceiptDocumentService receiptDocumentService,
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
                throw new EntityNotFoundException("ReceiptDocument", command.Id);

            // 2. Revert balance changes
            await receiptDocumentService.RevertReceiptBalanceChangesAsync(document, cancellationToken);

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