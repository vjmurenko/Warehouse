using MediatR;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;

public record DeleteShipmentCommand(Guid Id) : IRequest<Unit>;