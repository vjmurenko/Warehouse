using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Commands.DeleteClient;

public class DeleteClientCommandHandler(
    IClientService clientService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteClientCommand, Unit>
{
    public async Task<Unit> Handle(DeleteClientCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var success = await clientService.DeleteAsync(command.Id);
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