using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;

public class DeleteShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteShipmentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteShipmentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Получение документа
            var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new EntityNotFoundException("ShipmentDocument", command.Id);

            // 2. Проверка что документ не подписан
            if (document.IsSigned)
                throw new SignedDocumentException("delete", "shipment", document.Number);

            // 3. Удаление документа
            await shipmentRepository.DeleteAsync(document, cancellationToken);

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