using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Features.References.Queries;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.References;


public class GetActiveReferencesQueryHandler<T>(WarehouseDbContext context) : IRequestHandler<GetActiveReferencesQuery<T>, IEnumerable<T>>
    where T : Reference
{
    public async Task<IEnumerable<T>> Handle(GetActiveReferencesQuery<T> request, CancellationToken ctx)
    {
        return await context.Set<T>().Where(r => r.IsActive).ToListAsync(ctx);
    }
}