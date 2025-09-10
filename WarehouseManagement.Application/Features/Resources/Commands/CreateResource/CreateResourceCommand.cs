using MediatR;

namespace WarehouseManagement.Application.Features.Resources.Commands.CreateResource;

public record CreateResourceCommand(string Name) : IRequest<Guid>;