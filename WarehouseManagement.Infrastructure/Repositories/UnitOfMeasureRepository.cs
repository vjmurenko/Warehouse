using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class UnitOfMeasureRepository(WarehouseDbContext dbContext) : ReferenceRepository<UnitOfMeasure>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        var result = await DbContext.ReceiptResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx);
        return result;
    }
}