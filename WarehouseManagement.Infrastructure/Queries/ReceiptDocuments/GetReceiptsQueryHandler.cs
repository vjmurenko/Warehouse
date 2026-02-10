using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.ReceiptDocuments;

public sealed class GetReceiptsQueryHandler(WarehouseDbContext context) : IRequestHandler<GetReceiptsQuery, List<ReceiptDocumentDto>>
{
    public async Task<List<ReceiptDocumentDto>> Handle(GetReceiptsQuery request, CancellationToken ctx){
    
        var query = context.ReceiptDocuments
            .AsNoTracking()
            .AsSplitQuery()
            .AsQueryable();
            
        if (request.FromDate.HasValue)
        {
            var fromDateUtc = request.FromDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Utc)
                : request.FromDate.Value.ToUniversalTime();
            query = query.Where(r => r.Date >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = request.ToDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(r => r.Date <= toDateUtc);
        }

        if (request.DocumentNumbers is not null && request.DocumentNumbers.Any())
        {
            query = query.Where(r => request.DocumentNumbers.Contains(r.Number));
        }

        if (request.ResourceIds is not null && request.ResourceIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => request.ResourceIds.Contains(rr.ResourceId)));
        }

        if (request.UnitIds is not null && request.UnitIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => request.UnitIds.Contains(rr.UnitOfMeasureId)));
        }

        var result = await query
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Number)
            .Select(r => 
                new ReceiptDocumentDto(
                    r.Id,
                    r.Number,
                    r.Date,
                    r.ReceiptResources.Select(c => new ReceiptResourceDetailDto(
                        c.Id,
                        c.ResourceId,
                        c.Resource.Name,
                        c.UnitOfMeasureId,
                        c.UnitOfMeasure.Name,
                        c.Quantity))
                        .ToList()))
        
            .ToListAsync(ctx);
        
        return result;
    }
}