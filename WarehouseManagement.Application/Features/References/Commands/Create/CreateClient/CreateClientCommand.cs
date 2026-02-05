using MediatR;

namespace WarehouseManagement.Application.Features.References.Commands.Create.CreateClient;

public record CreateClientCommand(string Name, string Address) : IRequest<Guid>;
