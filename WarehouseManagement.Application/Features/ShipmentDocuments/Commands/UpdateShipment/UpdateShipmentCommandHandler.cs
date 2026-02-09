using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;

public sealed class UpdateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IReferenceValidationService referenceValidationService,
    IReferenceRepository<Client> clientRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateShipmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateShipmentCommand command, CancellationToken ctx)
    {
        var document = await shipmentRepository.GetByIdWithResourcesAsync(command.Id, ctx);
        if (document is null)
            throw new InvalidOperationException($"Документ с ID {command.Id} не найден");

        if (document.IsSigned)
            throw new InvalidOperationException("Подписанный документ отгрузки нельзя редактировать. Используйте команду отзыва документа.");

        if (await shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, ctx))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        await ValidateClient(command.ClientId, document.ClientId, ctx);
        await ValidateShipmentResourcesForUpdate(command.Resources, document, ctx);

        var newResources = command.Resources.Select(c => ShipmentResource.Create(command.Id, c.ResourceId, c.UnitId, c.Quantity)).ToList();
        
        document.Update(command.Number, command.ClientId, command.Date, newResources);
        
        if (command.Sign)
        {
            var items = newResources.Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity)).ToList();
            await balanceService.ValidateAvailability(items, ctx);
            
            var negativeItems = items.Select(i => (i.ResourceId, i.UnitOfMeasureId, -i.Quantity));
            await balanceService.UpdateBalances(negativeItems, ctx);
            
            document.Sign();
        }
        
        await unitOfWork.SaveChangesAsync(ctx);

        return Unit.Value;
    }
    
     private async Task ValidateShipmentResourcesForUpdate(
        List<ShipmentResourceDto> updatedShipmentResources,
        ShipmentDocument currentDocument,
        CancellationToken ctx)
    {
        var resourcesForExclude = new List<ShipmentResourceDto>();
        
            resourcesForExclude.AddRange(currentDocument.ShipmentResources
                .Select(c => new ShipmentResourceDto(c.ResourceId, c.UnitOfMeasureId, c.Quantity)));
            
        var resourcesToValidate = updatedShipmentResources
            .Where(u => !resourcesForExclude.Any(r => r.UnitId == u.UnitId && r.ResourceId == u.ResourceId))
            .ToList();
        
        await referenceValidationService.ValidateResourcesAsync(resourcesToValidate.Select(c => c.ResourceId), ctx);
        await referenceValidationService.ValidateUnitsAsync(resourcesToValidate.Select(c => c.UnitId), ctx);
    }

    private async Task ValidateClient(Guid clientId, Guid? currentClient, CancellationToken ctx)
    {
        var clients = await clientRepository.GetArchivedAsync(ctx);
        var archivedClient = clients.Where(c => c.Id != currentClient).SingleOrDefault(c => c.Id == clientId);

        if (archivedClient is not null)
        {
            throw new InvalidOperationException($"Клиент {archivedClient.Name} находится в архиве и не может быть использован");
        }
    }
}