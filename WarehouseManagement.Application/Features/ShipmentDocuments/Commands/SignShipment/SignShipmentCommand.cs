using MediatR;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.SignShipment;

public record SignShipmentCommand(Guid ShipmentDocumentId) : IRequest<bool>;
