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
            var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new EntityNotFoundException("ReceiptDocument", command.Id);

            await receiptDocumentService.RevertReceiptBalanceChangesAsync(document, cancellationToken);

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