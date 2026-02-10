using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ResourceRepository(WarehouseDbContext dbContext) : ReferenceRepository<Resource>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        var result = await DbContext.ReceiptResources
            .AnyAsync(c => c.ResourceId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.ResourceId == id, ctx);
        return result;
    }
}