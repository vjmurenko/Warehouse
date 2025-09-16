using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Adapters;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public class RevokeShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
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

        // 3. Восстановление баланса
        var deltas = document.ShipmentResources.Select(r => new ShipmentResourceAdapter(r).ToDelta()).ToList();
        await balanceService.IncreaseBalances(deltas, cancellationToken);
        
        // 4. Отзыв документа
        document.Revoke();

        // 5. Сохранение изменений
        shipmentRepository.Update(document);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}