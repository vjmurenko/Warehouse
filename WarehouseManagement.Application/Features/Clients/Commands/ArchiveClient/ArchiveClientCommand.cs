using MediatR;

namespace WarehouseManagement.Application.Features.Clients.Commands.ArchiveClient;

public record ArchiveClientCommand(Guid Id) : IRequest<Unit>;