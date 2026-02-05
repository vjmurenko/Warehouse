using MediatR;

namespace WarehouseManagement.Application.Features.References.Commands.Update.UpdateClient;

public record UpdateClientCommand(Guid ClientId,string Name, string Address) : IRequest;