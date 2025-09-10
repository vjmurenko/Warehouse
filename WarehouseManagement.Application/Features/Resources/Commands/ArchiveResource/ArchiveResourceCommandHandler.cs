using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Commands.ArchiveResource;

public class ArchiveResourceCommandHandler : IRequestHandler<ArchiveResourceCommand, Unit>
{
    private readonly IResourceService _resourceService;

    public ArchiveResourceCommandHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<Unit> Handle(ArchiveResourceCommand request, CancellationToken cancellationToken)
    {
        await _resourceService.ArchiveAsync(request.Id);
        return Unit.Value;
    }
}