using MediatR;

namespace WarehouseManagement.Application.Features.Resources.Commands.ArchiveResource;

public record ArchiveResourceCommand(Guid Id) : IRequest<Unit>;