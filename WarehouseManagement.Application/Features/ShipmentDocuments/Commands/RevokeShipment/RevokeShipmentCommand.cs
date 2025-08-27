using MediatR;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;

public record RevokeShipmentCommand(Guid Id) : IRequest<Unit>;