using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Commands.UpdateResource;

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, Unit>
{
    private readonly IResourceService _resourceService;

    public UpdateResourceCommandHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<Unit> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        await _resourceService.UpdateResourceAsync(request.Id, request.Name);
        return Unit.Value;
    }
}