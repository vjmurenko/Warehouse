using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;
using WarehouseManagement.Application.Features.References.Queries;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.References;

public class GetReferenceByIdQueryHandler<T>(WarehouseDbContext context) : IRequestHandler<GetReferenceByIdQuery<T>, T>
    where T : Reference
{
    public async Task<T> Handle(GetReferenceByIdQuery<T> request, CancellationToken ctx)
    {
        return await context.Set<T>().SingleAsync(r => r.Id == request.Id, ctx);
    }
}