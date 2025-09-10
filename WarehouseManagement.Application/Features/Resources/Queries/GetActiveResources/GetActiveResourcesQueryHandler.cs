using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetActiveResources;

public class GetActiveResourcesQueryHandler : IRequestHandler<GetActiveResourcesQuery, List<ResourceDto>>
{
    private readonly IResourceService _resourceService;

    public GetActiveResourcesQueryHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<List<ResourceDto>> Handle(GetActiveResourcesQuery request, CancellationToken cancellationToken)
    {
        var resources = await _resourceService.GetActiveAsync();
        return resources.Select(r => new ResourceDto(r.Id, r.Name, r.IsActive)).ToList();
    }
}