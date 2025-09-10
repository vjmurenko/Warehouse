using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetResources;

public class GetResourcesQueryHandler : IRequestHandler<GetResourcesQuery, List<ResourceDto>>
{
    private readonly IResourceService _resourceService;

    public GetResourcesQueryHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<List<ResourceDto>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        var resources = await _resourceService.GetAllAsync();
        return resources.Select(r => new ResourceDto(r.Id, r.Name, r.IsActive)).ToList();
    }
}