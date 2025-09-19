using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public class ClientRepository(WarehouseDbContext dbContext, ILogger<ClientRepository> logger) : NamedEntityRepository<Client>(dbContext)
{
    public override async Task<bool> IsUsingInDocuments(Guid id, CancellationToken ctx)
    {
        logger.LogInformation("Checking if client {ClientId} is used in documents", id);
        var result = await DbContext.ShipmentDocuments.AnyAsync(c => c.ClientId == id, ctx);
        logger.LogInformation("Client {ClientId} is used in documents: {IsUsed}", id, result);
        return result;
    }
}