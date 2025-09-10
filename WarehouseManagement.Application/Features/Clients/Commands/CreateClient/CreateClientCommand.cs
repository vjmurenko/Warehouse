using MediatR;

namespace WarehouseManagement.Application.Features.Clients.Commands.CreateClient;

public record CreateClientCommand(
    string Name,
    string Address
) : IRequest<Guid>;