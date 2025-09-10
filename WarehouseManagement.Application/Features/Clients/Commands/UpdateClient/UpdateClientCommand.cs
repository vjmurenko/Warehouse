using MediatR;

namespace WarehouseManagement.Application.Features.Clients.Commands.UpdateClient;

public record UpdateClientCommand(
    Guid Id,
    string Name,
    string Address
) : IRequest<Unit>;