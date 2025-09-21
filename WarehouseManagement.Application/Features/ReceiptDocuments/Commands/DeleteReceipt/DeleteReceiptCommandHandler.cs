using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReceiptCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document == null)
            throw new EntityNotFoundException("ReceiptDocument", command.Id);
        
        // Add domain event to handle balance decrease
        document.AddReceiptDeletedEvent();
      
        receiptRepository.Delete(document);
        await unitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}