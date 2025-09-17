using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public class ClientRepository(WarehouseDbContext dbContext) : NamedEntityRepository<Client>(dbContext)
{
    public override  async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
       return await DbContext.ShipmentDocuments.AnyAsync(c => c.ClientId == id, ctx);
    }
}