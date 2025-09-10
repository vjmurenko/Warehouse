using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Commands.CreateResource;

public class CreateResourceCommandHandler(
    IResourceService resourceService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateResourceCommand, Guid>
{
    public async Task<Guid> Handle(CreateResourceCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var resourceId = await resourceService.CreateResourceAsync(command.Name);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return resourceId;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}