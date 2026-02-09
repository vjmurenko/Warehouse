using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public sealed class DeleteReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReceiptCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new EntityNotFoundException("ReceiptDocument", command.Id);

        var items = document.ReceiptResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, -r.Quantity));
        await balanceService.UpdateBalances(items, cancellationToken);
      
        receiptRepository.Delete(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}