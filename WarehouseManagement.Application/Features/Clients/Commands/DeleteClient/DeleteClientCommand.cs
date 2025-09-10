using MediatR;

namespace WarehouseManagement.Application.Features.Clients.Commands.DeleteClient;

public record DeleteClientCommand(Guid Id) : IRequest<Unit>;