using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Adapters;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public class RevokeShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeShipmentCommand, Unit>
{
    public async Task<Unit> Handle(RevokeShipmentCommand command, CancellationToken cancellationToken)
    {
        // 1. Получение документа
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
        if (document == null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        // 2. Проверка что документ подписан
        if (!document.IsSigned)
            throw new InvalidOperationException("Документ не подписан и не может быть отозван");

        // 3. Отзыв документа (domain event will handle balance increase)
        document.Revoke();

        // 4. Сохранение изменений
        shipmentRepository.Update(document);

        await unitOfWork.SaveEntitiesAsync(cancellationToken);
        return Unit.Value;
    }
}