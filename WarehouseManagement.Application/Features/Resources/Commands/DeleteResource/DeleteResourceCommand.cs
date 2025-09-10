using MediatR;

namespace WarehouseManagement.Application.Features.Resources.Commands.DeleteResource;

public record DeleteResourceCommand(Guid Id) : IRequest<Unit>;