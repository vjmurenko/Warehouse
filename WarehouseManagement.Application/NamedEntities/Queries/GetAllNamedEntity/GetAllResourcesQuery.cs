using MediatR;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Queries.GetAllResources;

public class GetAllResourcesQuery : IRequest<List<Resource>>
{
    
}