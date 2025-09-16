using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Adapters;
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
            var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new EntityNotFoundException("ReceiptDocument", command.Id);
            
            var balanceDeltaToDecrease = document.ReceiptResources.Select(c => new ReceiptResourceAdapter(c).ToDelta());
            await balanceService.DecreaseBalances(balanceDeltaToDecrease, cancellationToken);
          
            receiptRepository.Delete(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        
    }
}