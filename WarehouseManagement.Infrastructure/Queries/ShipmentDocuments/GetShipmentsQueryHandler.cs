using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.ShipmentDocuments;

public sealed class GetShipmentsQueryHandler(WarehouseDbContext context) : IRequestHandler<GetShipmentsQuery, List<ShipmentDocumentDto>>
{
    public async Task<List<ShipmentDocumentDto>> Handle(GetShipmentsQuery request, CancellationToken ctx)
    {
        var query = context.ShipmentDocuments
            .AsNoTracking()
            .AsSplitQuery()
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            var fromDateUtc =  request.FromDate.Value.ToUniversalTime();
            query = query.Where(s => s.Date >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = request.ToDate.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(s => s.Date <= toDateUtc);
        }

        if (request.DocumentNumbers is {Count: > 0})
        {
            query = query.Where(s => request.DocumentNumbers.Contains(s.Number));
        }

        if (request.ClientIds is {Count: > 0})
        {
            query = query.Where(s => request.ClientIds.Contains(s.ClientId));
        }

        if (request.ResourceIds is {Count: > 0})
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => request.ResourceIds.Contains(sr.ResourceId)));
        }

        if (request.UnitIds is {Count: > 0})
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => request.UnitIds.Contains(sr.UnitOfMeasureId)));
        }

        var result = await query
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Number)
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
                    r.Quantity)).ToList()
            ))
            .ToListAsync(ctx);
        return result;
    }
}