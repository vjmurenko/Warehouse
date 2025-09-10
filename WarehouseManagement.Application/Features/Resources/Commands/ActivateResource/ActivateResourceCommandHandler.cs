using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Commands.ActivateResource;

public class ActivateResourceCommandHandler : IRequestHandler<ActivateResourceCommand, Unit>
{
    private readonly IResourceService _resourceService;

    public ActivateResourceCommandHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<Unit> Handle(ActivateResourceCommand request, CancellationToken cancellationToken)
    {
        await _resourceService.ActivateAsync(request.Id);
        return Unit.Value;
    }
}