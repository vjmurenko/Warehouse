using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetResourceById;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceDto?>
{
    private readonly IResourceService _resourceService;

    public GetResourceByIdQueryHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<ResourceDto?> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resourceService.GetByIdAsync(request.Id);
        return resource != null ? new ResourceDto(resource.Id, resource.Name, resource.IsActive) : null;
    }
}