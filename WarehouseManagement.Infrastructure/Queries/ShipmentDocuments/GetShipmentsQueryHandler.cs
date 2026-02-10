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
            var fromDateUtc = request.FromDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Utc)
                : request.FromDate.Value.ToUniversalTime();
            query = query.Where(s => s.Date >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = request.ToDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(s => s.Date <= toDateUtc);
        }

        if (request.DocumentNumbers is not null && request.DocumentNumbers.Any())
        {
            query = query.Where(s => request.DocumentNumbers.Contains(s.Number));
        }

        if (request.ClientIds is not null && request.ClientIds.Any())
        {
            query = query.Where(s => request.ClientIds.Contains(s.ClientId));
        }

        if (request.ResourceIds is not null && request.ResourceIds.Any())
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => request.ResourceIds.Contains(sr.ResourceId)));
        }

        if (request.UnitIds is not null && request.UnitIds.Any())
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