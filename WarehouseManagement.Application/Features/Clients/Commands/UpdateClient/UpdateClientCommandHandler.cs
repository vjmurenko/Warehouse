using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler(
    IClientService clientService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClientCommand, Unit>
{
    public async Task<Unit> Handle(UpdateClientCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var success = await clientService.UpdateClientAsync(command.Id, command.Name, command.Address);
            if (!success)
            {
                throw new InvalidOperationException($"Client with ID {command.Id} not found");
            }
            
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