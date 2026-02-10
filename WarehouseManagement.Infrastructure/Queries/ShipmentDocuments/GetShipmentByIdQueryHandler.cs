using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;

public sealed class GetShipmentByIdQueryHandler(WarehouseDbContext context) : IRequestHandler<GetShipmentByIdQuery, ShipmentDocumentDto?>
{
    public async Task<ShipmentDocumentDto?> Handle(GetShipmentByIdQuery request, CancellationToken ctx)
    {
        return  await context.ShipmentDocuments
            .AsNoTracking()
            .AsSplitQuery()
            .Where(d => d.Id == request.Id)
            .Select(d => new ShipmentDocumentDto(
                d.Id,
                d.Number,
                d.ClientId,
                d.Client.Name,
                d.Date,
                d.IsSigned,
                d.ShipmentResources.Select(r => new ShipmentResourceDetailDto(
                    r.Id,
                    r.ResourceId,
                    r.Resource.Name,
                    r.UnitOfMeasureId,
                    r.UnitOfMeasure.Name,
                    r.Quantity
                )).ToList()
            ))
            .SingleOrDefaultAsync(ctx);
    }
}