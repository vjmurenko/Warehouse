using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands;

public class DeleteReferenceCommandHandler<T> : IRequestHandler<DeleteReferenceCommand<T>> where T : Reference 
{
    private readonly IReferenceRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReferenceCommandHandler<T>> _logger;

    public DeleteReferenceCommandHandler(IReferenceRepository<T> repository, IUnitOfWork unitOfWork, ILogger<DeleteReferenceCommandHandler<T>> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteReferenceCommand<T> request, CancellationToken ctx)
    {
        if (await _repository.IsUsingInDocuments(request.Id, ctx))
        {
            _logger.LogWarning("Cannot delete entity of type {EntityType} with ID: {EntityId} because it is in use in documents", typeof(T).Name, request.Id);
            throw new EntityInUseException(typeof(T).Name, request.Id, "documents");
        }

        var reference = await _repository.GetByIdAsync(request.Id, ctx);
        if (reference is null)
        {
            _logger.LogWarning("Entity of type {EntityType} with ID: {EntityId} not found for deletion", typeof(T).Name, request.Id);
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }

        _repository.Delete(reference);
        _logger.LogInformation("Entity of type {EntityType} with ID: {EntityId} marked for deletion", typeof(T).Name, request.Id);

        await _unitOfWork.SaveEntitiesAsync(ctx);
    }
}