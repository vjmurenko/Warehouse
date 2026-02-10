using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Activate;

public class ArchiveReferenceCommandHandler<T>(IReferenceRepository<T> repository, IUnitOfWork unitOfWork) : IRequestHandler<ArchiveReferenceCommand<T>>
    where T : Reference
{
    public async Task Handle(ArchiveReferenceCommand<T> request, CancellationToken ctx)
    {
        var reference = await repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        reference.Archive();

        await unitOfWork.SaveChangesAsync(ctx);
    }
}