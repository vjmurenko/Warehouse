using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.References.Commands.Update;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands;

public class UpdateReferenceCommandHandler<T>(IReferenceRepository<T> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateReferenceCommand<T>>
    where T : Reference
{
    public async Task Handle(UpdateReferenceCommand<T> request, CancellationToken ctx)
    {
        if (await repository.ExistsWithNameAsync(request.Name, request.Id, ctx))
        {
            throw new DuplicateEntityException(typeof(T).Name, request.Name);
        }

        var reference = await repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is  null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        
        reference.Rename(request.Name);
        
        await unitOfWork.SaveChangesAsync(ctx);
    }
}