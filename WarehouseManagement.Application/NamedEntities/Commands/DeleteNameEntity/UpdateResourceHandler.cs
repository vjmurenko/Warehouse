using MediatR;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Resources.Commands.UpdateResource;

public class UpdateResourceHandler(RepositoryBase<Resource> repository) : IRequestHandler<UpdateResourceCommand, bool>
{
    public async Task<bool> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await repository.GetByIdAsync(request.Id);
        if (resource == null)
        {
            throw new KeyNotFoundException();
        }

        return await repository.UpdateAsync(resource);
    }
}