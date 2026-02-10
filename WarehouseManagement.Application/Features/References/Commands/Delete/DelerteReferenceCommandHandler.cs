using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Delete;

public class DeleteReferenceCommandHandler<T>(IReferenceRepository<T> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteReferenceCommand<T>>
    where T : Reference
{
    public async Task Handle(DeleteReferenceCommand<T> request, CancellationToken ctx)
    {
        if (await repository.IsUsingInDocuments(request.Id, ctx))
        {
            throw new EntityInUseException(typeof(T).Name, request.Id, "documents");
        }

        var reference = await repository.GetByIdAsync(request.Id, ctx);
        if (reference is null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }

        repository.Delete(reference);

        await unitOfWork.SaveChangesAsync(ctx);
    }
}