using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Queries.GetAllResources;

public class GetAllResourcesHandler(IBaseRepository<Resource> repository) : IRequestHandler<GetAllResourcesQuery, List<Resource>>
{
    public async Task<List<Resource>> Handle(GetAllResourcesQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAll();
    }
}