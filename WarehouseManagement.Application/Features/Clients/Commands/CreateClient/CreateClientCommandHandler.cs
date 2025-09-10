using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler(
    IClientService clientService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> Handle(CreateClientCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var clientId = await clientService.CreateClientAsync(command.Name, command.Address);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return clientId;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}