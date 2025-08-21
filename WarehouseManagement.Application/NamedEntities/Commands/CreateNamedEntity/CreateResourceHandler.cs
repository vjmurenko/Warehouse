using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Commands.CreateResource;

public class CreateResourceHandler(IBaseRepository<Resource> repository) : IRequestHandler<CreateResourceCommand>
{
    private IBaseRepository<Resource> _repository { get; set; } = repository;

    public async Task Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = new Resource(request.Name);
        var result =  await _repository.CreateAsync(resource);
    }
}