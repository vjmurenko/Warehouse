using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IReferenceRepository<Client> clientRepository,
    IReferenceValidationService referenceValidationService,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken ctx)
    {
        if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: ctx))
            throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

        await ValidateClient(command.ClientId, ctx: ctx);
        await ValidateResources(command.Resources, ctx);
       
        var documentId = Guid.NewGuid();

        var resources = command.Resources
            .Where(r => r.Quantity > 0)
            .Select(r => ShipmentResource.Create(documentId, r.ResourceId, r.UnitId, r.Quantity))
            .ToList();

        var shipmentDocument = ShipmentDocument.Create(command.Number, command.ClientId, command.Date, resources);
        
        if (command.Sign)
        {
            var items = resources.Select(r => (r.ResourceId, r.UnitOfMeasureId, r.Quantity)).ToList();
            await balanceService.ValidateAvailability(items, ctx);
            
            var negativeItems = items.Select(i => (i.ResourceId, i.UnitOfMeasureId, -i.Quantity));
            await balanceService.UpdateBalances(negativeItems, ctx);
            
            shipmentDocument.Sign();
        }

        shipmentRepository.Create(shipmentDocument);

        await unitOfWork.SaveChangesAsync(ctx);
        return shipmentDocument.Id;
    }
    
    private async Task ValidateResources(List<ShipmentResourceDto> resources, CancellationToken ctx)
    {
        await referenceValidationService.ValidateResourcesAsync(resources.Select(r => r.ResourceId), ctx);
        await referenceValidationService.ValidateUnitsAsync(resources.Select(r => r.UnitId), ctx);
    }
    
    private async Task ValidateClient(Guid clientId, CancellationToken ctx = default)
    {
        var clients = await clientRepository.GetArchivedAsync(ctx);

        var client = clients.SingleOrDefault(c => c.Id == clientId);
        if (client is not null)
        {
            throw new InvalidOperationException($"Клиент {client.Name} находится в архиве и не может быть использован");
        }
    }
}
