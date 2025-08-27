using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public class RevokeShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeShipmentCommand, Unit>
{
    public async Task<Unit> Handle(RevokeShipmentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Получение документа
            var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, cancellationToken);
            if (document == null)
                throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

            // 2. Проверка что документ подписан
            if (!document.IsSigned)
                throw new InvalidOperationException("Документ не подписан и не может быть отозван");

            // 3. Восстановление баланса
            foreach (var resource in document.ShipmentResources)
            {
                await balanceService.IncreaseBalance(
                    resource.ResourceId,
                    resource.UnitOfMeasureId,
                    resource.Quantity,
                    cancellationToken);
            }

            // 4. Отзыв документа
            document.Revoke();

            // 5. Сохранение изменений
            await shipmentRepository.UpdateAsync(document, cancellationToken);

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