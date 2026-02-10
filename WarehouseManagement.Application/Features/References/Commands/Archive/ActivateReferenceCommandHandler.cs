using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Activate;

public class ActivateReferenceCommandHandler<T>(IReferenceRepository<T> repository, IUnitOfWork unitOfWork) : IRequestHandler<ActivateReferenceCommand<T>>
    where T : Reference
{
    public async Task Handle(ActivateReferenceCommand<T> request, CancellationToken ctx)
    {
        var reference = await repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        reference.Activate();

        await unitOfWork.SaveChangesAsync(ctx);
    }
}