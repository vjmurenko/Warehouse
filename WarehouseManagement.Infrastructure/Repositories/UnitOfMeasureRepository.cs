using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class UnitOfMeasureRepository(WarehouseDbContext dbContext, ILogger<UnitOfMeasureRepository> logger) : NamedEntityRepository<UnitOfMeasure>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        logger.LogInformation("Checking if unit of measure {UnitOfMeasureId} is used in documents", id);
        var result = await DbContext.ReceiptResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx);
        logger.LogInformation("Unit of measure {UnitOfMeasureId} is used in documents: {IsUsed}", id, result);
        return result;
    }
}