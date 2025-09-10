using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IReceiptDocumentService receiptDocumentService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateReceiptCommand, Unit>
{
    public async Task<Unit> Handle(UpdateReceiptCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingDocument = await GetExistingDocumentAsync(command.Id, cancellationToken);
            
            await receiptDocumentService.ValidateReceiptRequestAsync(
                command.Number, command.Resources, command.Id, cancellationToken);

            var oldResources = CreateResourceSnapshot(existingDocument);
            
            UpdateDocument(existingDocument, command);
            
            await receiptDocumentService.ApplyBalanceChangesForUpdateAsync(
                oldResources, existingDocument.ReceiptResources, cancellationToken);
            
            await receiptRepository.UpdateAsync(existingDocument, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Unit.Value;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ReceiptDocument> GetExistingDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(id, cancellationToken);
        if (document == null)
            throw new InvalidOperationException($"Документ с ID {id} не найден");
        return document;
    }

    private List<(Guid ResourceId, Guid UnitId, decimal Quantity)> CreateResourceSnapshot(ReceiptDocument document)
    {
        return document.ReceiptResources
            .Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity.Value))
            .ToList();
    }

    private void UpdateDocument(ReceiptDocument document, UpdateReceiptCommand command)
    {
        document.UpdateNumber(command.Number);
        document.UpdateDate(command.Date);
        document.ClearResources();

        foreach (var dto in command.Resources)
        {
            document.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
        }
    }
}