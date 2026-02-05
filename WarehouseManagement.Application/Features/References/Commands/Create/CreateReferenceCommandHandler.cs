using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Create;

public class CreateReferenceCommandHandler<T> : IRequestHandler<CreateReferenceCommand<T>, Guid> where T : Reference
{
    private readonly IReferenceRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReferenceCommandHandler<T>> _logger;

    public CreateReferenceCommandHandler(IReferenceRepository<T> repository, IUnitOfWork unitOfWork, ILogger<CreateReferenceCommandHandler<T>> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateReferenceCommand<T> request, CancellationToken ctx)
    { ;
        if (await _repository.ExistsWithNameAsync(request.Name, ctx: ctx))
        {
            _logger.LogWarning("Duplicate entity detected for type {EntityType} with name: {EntityName}", typeof(T).Name, request.Name);
            throw new DuplicateEntityException(typeof(T).Name, request.Name);
        }
        
        var reference = CreateInstance<T>(request.Name);
        var id = _repository.Create(reference);
        _logger.LogInformation("Entity created with temporary ID: {EntityId}", id);

        await _unitOfWork.SaveChangesAsync(ctx);
        _logger.LogInformation("Entity of type {EntityType} successfully saved with ID: {EntityId}", typeof(T).Name, id);
        
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