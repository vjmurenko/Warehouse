using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public class CreateReceiptHandler : IRequestHandler<CreateReceiptCommand, Guid>
{
    private readonly WarehouseDbContext _context;

    public CreateReceiptHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        // Проверка уникальности номера документа
        var existingDocument = await _context.Set<ReceiptDocument>()
            .FirstOrDefaultAsync(d => d.Number == request.Number, cancellationToken);

        if (existingDocument != null)
        {
            throw new InvalidOperationException($"Receipt document with number '{request.Number}' already exists.");
        }

        // Создание документа поступления
        var receiptDocument = new ReceiptDocument(request.Number, request.Date);

        // Добавление ресурсов поступления
        if (request.Resources != null && request.Resources.Any())
        {
            foreach (var resourceDto in request.Resources)
            {
                var receiptResource = new ReceiptResource(
                    receiptDocument.Id,
                    resourceDto.ResourceId,
                    resourceDto.UnitOfMeasureId,
                    new Quantity(resourceDto.Quantity));

                receiptDocument.AddResource(receiptResource);
            }
        }

        _context.Set<ReceiptDocument>().Add(receiptDocument);
        await _context.SaveChangesAsync(cancellationToken);

        // Обновление баланса
        await UpdateBalanceForReceipt(receiptDocument, cancellationToken);

        return receiptDocument.Id;
    }

    private async Task UpdateBalanceForReceipt(ReceiptDocument receiptDocument, CancellationToken cancellationToken)
    {
        foreach (var receiptResource in receiptDocument.ReceiptResources)
        {
            var balance = await _context.Set<Balance>()
                .FirstOrDefaultAsync(b =>
                    b.ResourceId == receiptResource.ResourceId &&
                    b.UnitOfMeasureId == receiptResource.UnitOfMeasureId,
                    cancellationToken);

            if (balance == null)
            {
                // Создаем новую запись баланса
                balance = new Balance(
                    receiptResource.ResourceId,
                    receiptResource.UnitOfMeasureId,
                    receiptResource.Quantity);

                _context.Set<Balance>().Add(balance);
            }
            else
            {
                // Увеличиваем существующий баланс
                balance.Increase(receiptResource.Quantity);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
