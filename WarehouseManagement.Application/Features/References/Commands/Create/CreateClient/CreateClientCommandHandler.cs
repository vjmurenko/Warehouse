using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.References.Commands;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Create.CreateClient;

public class CreateClientCommandHandler(IReferenceRepository<Client> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> Handle(CreateClientCommand request, CancellationToken ctx)
    {
       
        if (await repository.ExistsWithNameAsync(request.Name, ctx: ctx))
        {
            throw new DuplicateEntityException(nameof(Client), request.Name);
        }

        var client = Client.Create(request.Name, new Address(request.Address));
        var id = repository.Create(client);

        await unitOfWork.SaveChangesAsync(ctx);
        
        return id;
    }
}
