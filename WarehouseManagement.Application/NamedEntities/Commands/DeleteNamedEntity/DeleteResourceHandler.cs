using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Commands.DeleteResource;

public class DeleteResourceHandler(IBaseRepository<Resource> repository) : IRequestHandler<DeleteResourceCommand, bool>
{
    public async Task<bool> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        return await repository.DeleteAsync(request.Resource);
    }
}