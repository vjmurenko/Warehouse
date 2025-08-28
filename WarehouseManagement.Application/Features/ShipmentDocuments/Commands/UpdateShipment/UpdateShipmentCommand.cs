using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;

public record UpdateShipmentCommand(
    Guid Id,
    string Number,
    Guid ClientId,
    DateTime Date,
    List<ShipmentResourceDto> Resources,
    bool Sign = false
) : IRequest<Unit>;