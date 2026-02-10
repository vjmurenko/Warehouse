using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.ReceiptDocuments;

public sealed class GetReceiptByIdQueryHandler(WarehouseDbContext context) : IRequestHandler<GetReceiptByIdQuery, ReceiptDocumentDto?>
{
    public async Task<ReceiptDocumentDto?> Handle(GetReceiptByIdQuery query, CancellationToken ctx)
    {
        return await context.ReceiptDocuments
            .AsSplitQuery()
            .Where(r => r.Id == query.Id)
            .Select(c => new ReceiptDocumentDto(
                c.Id,
                c.Number,
                c.Date,
                c.ReceiptResources.Select(r => new ReceiptResourceDetailDto(
                    r.Id,
                    r.ResourceId,
                    r.Resource.Name,
                    r.UnitOfMeasureId,
                    r.UnitOfMeasure.Name,
                    r.Quantity)).ToList()))
            .SingleOrDefaultAsync(ctx);
    }
}