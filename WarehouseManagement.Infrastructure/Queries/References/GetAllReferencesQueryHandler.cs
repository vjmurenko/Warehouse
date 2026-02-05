using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WarehouseManagement.Application.Features.References.Queries;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Queries.References;

public class GetAllReferencesQueryHandler<T>(WarehouseDbContext context) : IRequestHandler<GetAllReferencesQuery<T>, IEnumerable<T>>
    where T : Reference
{
    public async Task<IEnumerable<T>> Handle(GetAllReferencesQuery<T> request, CancellationToken ctx)
    {
        return await context.Set<T>().ToListAsync(ctx);
    }
}