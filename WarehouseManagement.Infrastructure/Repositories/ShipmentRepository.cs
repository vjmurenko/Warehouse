using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ShipmentRepository(WarehouseDbContext context, ILogger<ShipmentRepository> logger) : RepositoryBase<ShipmentDocument>(context), IShipmentRepository
{
    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if shipment document with number {Number} exists, excluding ID {ExcludeId}", number, excludeId);
        var query = context.ShipmentDocuments.Where(s => s.Number == number);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        
        var result = await query.AnyAsync(cancellationToken);
        logger.LogInformation("Shipment document with number {Number} exists: {Exists}", number, result);
        return result;
    }

    public async Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting shipment document with ID {Id} including resources", id);
        
        var local = context.ShipmentDocuments.Local.SingleOrDefault(s => s.Id == id);
        if (local is not null)
        {
            logger.LogInformation("Shipment document with ID {Id} found in local cache", id);
            return local;
        }
        
        var result = await context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        logger.LogInformation("Shipment document with ID {Id} found: {Found}", id, result is not null);
        return result;
    }
}