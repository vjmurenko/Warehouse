using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ResourceRepository(WarehouseDbContext dbContext, ILogger<ResourceRepository> logger) : NamedEntityRepository<Resource>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        logger.LogInformation("Checking if resource {ResourceId} is used in documents", id);
        var result = await DbContext.ReceiptResources
            .AnyAsync(c => c.ResourceId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.ResourceId == id, ctx);
        logger.LogInformation("Resource {ResourceId} is used in documents: {IsUsed}", id, result);
        return result;
    }
}