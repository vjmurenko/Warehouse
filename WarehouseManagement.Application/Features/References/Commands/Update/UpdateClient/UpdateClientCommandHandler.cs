using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Update.UpdateClient;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand>
{
    private readonly IReferenceRepository<Client> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateClientCommandHandler(IReferenceRepository<Client> repository, IUnitOfWork unitOfWork, ILogger<UpdateClientCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateClientCommand request, CancellationToken ctx)
    {
        if (await _repository.ExistsWithNameAsync(request.Name, request.ClientId, ctx))
        {
            _logger.LogWarning("Duplicate reference detected for type {EntityType} with name: {EntityName} during update", nameof(Client), request.Name);
            throw new DuplicateEntityException(nameof(Client), request.Name);
        }

        var client = await _repository.GetByIdAsync(request.ClientId, ctx);

        client.Rename(request.Name);
        client.ChangeAddress(new Address(request.Address));

        await _unitOfWork.SaveEntitiesAsync(ctx);
    }
}