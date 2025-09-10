using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetResources;

public class GetResourcesQueryHandler(IResourceService resourceService) : IRequestHandler<GetResourcesQuery, List<ResourceDto>>
{
    public async Task<List<ResourceDto>> Handle(GetResourcesQuery query, CancellationToken cancellationToken)
    {
        var resources = await resourceService.GetAllAsync();
        return resources.Select(r => new ResourceDto(
            r.Id,
            r.Name,
            r.IsActive
        )).ToList();
    }
}