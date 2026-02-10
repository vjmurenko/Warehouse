using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Create;

public class CreateReferenceCommandHandler<T>(IReferenceRepository<T> repository, IUnitOfWork unitOfWork) : IRequestHandler<CreateReferenceCommand<T>, Guid> where T : Reference
{
    public async Task<Guid> Handle(CreateReferenceCommand<T> request, CancellationToken ctx)
    { ;
        if (await repository.ExistsWithNameAsync(request.Name, ctx: ctx))
        {
            throw new DuplicateEntityException(typeof(T).Name, request.Name);
        }
        
        var reference = CreateInstance<T>(request.Name);
        var id = repository.Create(reference);

        await unitOfWork.SaveChangesAsync(ctx);
        
        return id;
    }

    private static T CreateInstance<T>(string name) where T : Reference
    {
        if (typeof(T) == typeof(Resource))
        {
            return (T)(object)Resource.Create(name);
        } 
        if (typeof(T) == typeof(UnitOfMeasure))
        {
            return (T)(object)UnitOfMeasure.Create(name);
        }
        
        throw new InvalidOperationException($"Cannot create instance of type {typeof(T)}");
    }
}