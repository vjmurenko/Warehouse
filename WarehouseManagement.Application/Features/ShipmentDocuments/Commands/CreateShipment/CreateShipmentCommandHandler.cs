using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IReceiptValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Проверка уникальности номера
            if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

            // 2. Создание документа
            var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);

            // 3. Валидация и добавление ресурсов (проверяем всё до итерации)
            foreach (var dto in command.Resources)
            {
                // Валидация через validation service
                await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
                await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
                
                // Добавление через доменную модель
                shipmentDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }
            
            // 3.1. Проверка что документ не пустой (бизнес-правило)
            shipmentDocument.ValidateNotEmpty();

            // 4. Подписание документа если требуется (с проверкой доступности баланса)
            if (command.SignImmediately)
            {
                // Проверка баланса и списание - DecreaseBalance автоматически проверяет наличие
                foreach (var resource in shipmentDocument.ShipmentResources)
                {
                    await balanceService.DecreaseBalance(
                        resource.ResourceId,
                        resource.UnitOfMeasureId,
                        resource.Quantity,
                        cancellationToken);
                }
                shipmentDocument.Sign();
            }

            // 5. Сохранение документа
            await shipmentRepository.AddAsync(shipmentDocument, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return shipmentDocument.Id;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}