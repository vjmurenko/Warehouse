using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.Commands.DeleteResource;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, Unit>
{
    private readonly IResourceService _resourceService;

    public DeleteResourceCommandHandler(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<Unit> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        await _resourceService.DeleteAsync(request.Id);
        return Unit.Value;
    }
}