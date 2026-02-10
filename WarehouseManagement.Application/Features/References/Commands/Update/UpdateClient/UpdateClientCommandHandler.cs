using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Update.UpdateClient;

public class UpdateClientCommandHandler(IReferenceRepository<Client> repository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateClientCommand>
{
    public async Task Handle(UpdateClientCommand request, CancellationToken ctx)
    {
        if (await repository.ExistsWithNameAsync(request.Name, request.ClientId, ctx))
        {
            throw new DuplicateEntityException(nameof(Client), request.Name);
        }

        var client = await repository.GetByIdAsync(request.ClientId, ctx);

        client.Update(request.Name, new Address(request.Address));
        
        await unitOfWork.SaveChangesAsync(ctx);
    }
}