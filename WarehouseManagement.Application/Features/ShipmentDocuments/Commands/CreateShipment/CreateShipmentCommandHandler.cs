using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public class CreateShipmentCommandHandler(
    IShipmentRepository shipmentRepository,
    IBalanceService balanceService,
    IShipmentValidationService validationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (await shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"Документ с номером {command.Number} уже существует");

            await validationService.ValidateClient(command.ClientId);
            
            var shipmentDocument = new ShipmentDocument(command.Number, command.ClientId, command.Date);
            
            
            await validationService.ValidateShipmentResourcesForUpdate(command.Resources, cancellationToken);
            
            foreach (var dto in command.Resources)
            {
                shipmentDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }
            
            shipmentDocument.ValidateNotEmpty();

            foreach (var resource in shipmentDocument.ShipmentResources)
            {
                await balanceService.ValidateBalanceAvailability(
                    resource.ResourceId,
                    resource.UnitOfMeasureId,
                    resource.Quantity,
                    cancellationToken);
            }

            if (command.Sign)
            {
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