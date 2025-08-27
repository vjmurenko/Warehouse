using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;

public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDocumentDto?>;