using MediatR;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.References.Commands;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Create.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Guid>
{
    private readonly IReferenceRepository<Client> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public CreateClientCommandHandler(IReferenceRepository<Client> repository, IUnitOfWork unitOfWork, ILogger<CreateClientCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateClientCommand request, CancellationToken ctx)
    {
       
        if (await _repository.ExistsWithNameAsync(request.Name, ctx: ctx))
        {
            _logger.LogWarning("Duplicate entity detected for Client with name: {EntityName}", request.Name);
            throw new DuplicateEntityException(nameof(Client), request.Name);
        }

        var client = Client.Create(request.Name, new Address(request.Address));
        var id = _repository.Create(client);
        _logger.LogInformation("Client created with temporary ID: {EntityId}", id);

        await _unitOfWork.SaveChangesAsync(ctx);
        _logger.LogInformation("Client successfully saved with ID: {EntityId}", id);
        
        return id;
    }
}
