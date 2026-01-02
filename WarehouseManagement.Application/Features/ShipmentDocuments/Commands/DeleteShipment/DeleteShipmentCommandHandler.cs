using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;

public sealed class DeleteShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteShipmentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteShipmentCommand command, CancellationToken cancellationToken)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document is null)
            throw new EntityNotFoundException("ShipmentDocument", command.Id);

        if (document.IsSigned)
            throw new SignedDocumentException("delete", "shipment", document.Number);

        shipmentRepository.Delete(document);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}