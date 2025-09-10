using MediatR;

namespace WarehouseManagement.Application.Features.Resources.Commands.UpdateResource;

public record UpdateResourceCommand(
    Guid Id,
    string Name
) : IRequest<Unit>;