using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public sealed class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReceiptCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new EntityNotFoundException("ReceiptDocument", command.Id);
        
        document.Delete();
      
        receiptRepository.Delete(document);
        await unitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}