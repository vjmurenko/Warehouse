﻿using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public class CreateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IBalanceService balanceService,
    INamedEntityValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReceiptCommand, Guid>
{
    public async Task<Guid> Handle(CreateReceiptCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Проверка уникальности номера
            if (await receiptRepository.ExistsByNumberAsync(command.Number))
                throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

            // 2. Создание документа
            var receiptDocument = new ReceiptDocument(command.Number, command.Date);

            // 3. Валидация и добавление ресурсов
            foreach (var dto in command.Resources)
            {
                // Валидация через validation service
                await validationService.ValidateResourceAsync(dto.ResourceId, cancellationToken);
                await validationService.ValidateUnitOfMeasureAsync(dto.UnitId, cancellationToken);
                
                // Добавление через доменную модель
                receiptDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }

            // 4. Сохранение документа
            await receiptRepository.AddAsync(receiptDocument, cancellationToken);

            // 5. Обновление баланса
            foreach (var resource in receiptDocument.ReceiptResources)
            {
                await balanceService.IncreaseBalance(
                    resource.ResourceId,
                    resource.UnitOfMeasureId,
                    resource.Quantity,
                    cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return receiptDocument.Id;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}