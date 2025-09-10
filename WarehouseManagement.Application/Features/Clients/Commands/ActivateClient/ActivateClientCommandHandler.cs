using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Commands.ActivateClient;

public class ActivateClientCommandHandler(
    IClientService clientService,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateClientCommand, Unit>
{
    public async Task<Unit> Handle(ActivateClientCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var success = await clientService.ActivateAsync(command.Id);
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