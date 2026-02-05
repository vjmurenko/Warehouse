using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.References.Commands.Update;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands;

public class UpdateReferenceCommandHandler<T> : IRequestHandler<UpdateReferenceCommand<T>> where T : Reference
{
    private readonly IReferenceRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReferenceCommandHandler<T>> _logger;

    public UpdateReferenceCommandHandler(IReferenceRepository<T> repository, IUnitOfWork unitOfWork, ILogger<UpdateReferenceCommandHandler<T>> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateReferenceCommand<T> request, CancellationToken ctx)
    {
        if (await _repository.ExistsWithNameAsync(request.Name, request.Id, ctx))
        {
            _logger.LogWarning("Duplicate reference detected for type {EntityType} with name: {EntityName} during update", typeof(T).Name, request.Name);
            throw new DuplicateEntityException(typeof(T).Name, request.Name);
        }

        var reference = await _repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is  null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        
        reference.Rename(request.Name);
        
        await _unitOfWork.SaveEntitiesAsync(ctx);
    }
}