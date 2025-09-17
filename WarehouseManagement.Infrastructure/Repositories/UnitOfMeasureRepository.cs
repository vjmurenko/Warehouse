using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public class UnitOfMeasureRepository(WarehouseDbContext dbContext) : NamedEntityRepository<UnitOfMeasure>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        return await DbContext.ReceiptResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx) || await DbContext.ShipmentResources
            .AnyAsync(c => c.UnitOfMeasureId == id, ctx);
    }
}