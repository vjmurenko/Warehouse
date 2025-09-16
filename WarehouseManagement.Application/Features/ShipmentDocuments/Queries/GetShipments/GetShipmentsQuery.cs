using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;

public record GetShipmentsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    List<string>? DocumentNumbers = null,
    List<Guid>? ResourceIds = null,
    List<Guid>? UnitIds = null,
    List<Guid>? ClientIds = null
) : IRequest<List<ShipmentDocumentDto>>;