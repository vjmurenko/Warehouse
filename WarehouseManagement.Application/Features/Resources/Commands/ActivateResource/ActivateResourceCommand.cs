using MediatR;

namespace WarehouseManagement.Application.Features.Resources.Commands.ActivateResource;

public record ActivateResourceCommand(Guid Id) : IRequest<Unit>;