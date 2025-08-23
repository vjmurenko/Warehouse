using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public record CreateShipmentCommand(
    string Number,
    Guid ClientId,
    DateTime Date,
    List<CreateShipmentResourceDto> Resources
) : IRequest<Guid>;
