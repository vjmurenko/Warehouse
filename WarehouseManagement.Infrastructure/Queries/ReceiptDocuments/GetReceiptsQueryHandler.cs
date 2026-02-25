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
            var fromDateUtc = request.FromDate.Value.ToUniversalTime();
            query = query.Where(r => r.Date >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = request.ToDate.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(r => r.Date < toDateUtc);
        }

        if (request.DocumentNumbers is {Count: > 0})
        {
            query = query.Where(r => request.DocumentNumbers.Contains(r.Number));
        }

        if (request.ResourceIds is {Count: > 0})
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => request.ResourceIds.Contains(rr.ResourceId)));
        }

        if (request.UnitIds is {Count: > 0})
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