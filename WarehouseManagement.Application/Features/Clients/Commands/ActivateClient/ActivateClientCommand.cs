using MediatR;

namespace WarehouseManagement.Application.Features.Clients.Commands.ActivateClient;

public record ActivateClientCommand(Guid Id) : IRequest<Unit>;