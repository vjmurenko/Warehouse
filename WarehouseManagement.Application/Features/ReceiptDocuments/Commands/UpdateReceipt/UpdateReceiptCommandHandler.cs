using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public sealed class UpdateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    INamedEntityValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateReceiptCommand, Unit>
{
    public async Task<Unit> Handle(UpdateReceiptCommand command, CancellationToken ct)
    {
        var document = await receiptRepository.GetByIdWithResourcesAsync(command.Id, ct);
        if (document is null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (await receiptRepository.ExistsByNumberAsync(command.Number, command.Id, ct))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        var newResources = GetNewResources(document, command.Resources);
        if (newResources.Any())
        {
            await validationService.ValidateResourcesAsync(newResources.Select(r => r.ResourceId), ct);
            await validationService.ValidateUnitsAsync(newResources.Select(r => r.UnitId), ct);
        }
        
        document.UpdateNumber(command.Number);
        document.UpdateDate(command.Date);
        document.UpdateResources(command.Resources.Select(r => new BalanceDelta(r.ResourceId, r.UnitId, r.Quantity)));
        
        receiptRepository.Update(document);
        await unitOfWork.SaveEntitiesAsync(ct);

        return Unit.Value;
    }

    private List<ReceiptResourceDto> GetNewResources(ReceiptDocument document, List<ReceiptResourceDto> commandResources)
    {
        var existingKeys = document.ReceiptResources
            .Select(r => new { r.ResourceId, r.UnitOfMeasureId })
            .ToHashSet();

        return commandResources
            .Where(r => !existingKeys.Contains(new { r.ResourceId, UnitOfMeasureId = r.UnitId }))
            .ToList();
    }
}