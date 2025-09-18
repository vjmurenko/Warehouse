using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
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
            
            var balanceDeltaToDecrease = document.ReceiptResources
                .GroupBy(r => new {r.ResourceId, r.UnitOfMeasureId})
                .Select(c =>  new BalanceDelta(c.Key.ResourceId, c.Key.UnitOfMeasureId, c.Sum(r => r.Quantity.Value)));
            
            await balanceService.DecreaseBalances(balanceDeltaToDecrease, cancellationToken);
          
            receiptRepository.Delete(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
    }
}