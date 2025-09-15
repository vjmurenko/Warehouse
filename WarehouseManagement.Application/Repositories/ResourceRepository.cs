using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class ResourceRepository(WarehouseDbContext dbContext) : NamedEntityRepository<Resource>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        return await DbContext.ReceiptResources
            .AnyAsync(c => c.ResourceId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.ResourceId == id, ctx);
    }
}