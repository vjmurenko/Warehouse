using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, Guid>
{
    private readonly WarehouseDbContext _context;

    public CreateShipmentHandler(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        // Проверка уникальности номера документа
        var existingDocument = await _context.ShipmentDocuments
            .FirstOrDefaultAsync(d => d.Number == request.Number, cancellationToken);

        if (existingDocument != null)
        {
            throw new InvalidOperationException($"Shipment document with number '{request.Number}' already exists.");
        }

        // Проверка наличия ресурсов
        if (!request.Resources.Any())
        {
            throw new InvalidOperationException("Shipment document cannot be empty.");
        }

        // Создание документа отгрузки
        var shipmentDocument = new ShipmentDocument(request.Number, request.ClientId, request.Date);

        // Добавление ресурсов отгрузки
        foreach (var resourceDto in request.Resources)
        {
            var shipmentResource = new ShipmentResource(
                resourceDto.ResourceId,
                resourceDto.UnitOfMeasureId,
                new Quantity(resourceDto.Quantity));

            shipmentDocument.AddResource(shipmentResource);
        }
        
        _context.ShipmentDocuments.Add(shipmentDocument);
        await _context.SaveChangesAsync(cancellationToken);

        return shipmentDocument.Id;
    }
}
