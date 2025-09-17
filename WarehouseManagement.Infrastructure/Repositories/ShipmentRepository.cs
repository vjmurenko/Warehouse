using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public class ShipmentRepository(WarehouseDbContext context) : RepositoryBase<ShipmentDocument>(context), IShipmentRepository
{
    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.ShipmentDocuments.Where(s => s.Number == number);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<ShipmentDocument>> GetFilteredAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        List<Guid>? clientIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .AsQueryable();
        
        if (fromDate.HasValue)
        {
            var fromDateUtc = fromDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
                : fromDate.Value.ToUniversalTime();
            query = query.Where(s => s.Date >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = toDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : toDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(s => s.Date <= toDateUtc);
        }
        
        if (documentNumbers != null && documentNumbers.Any())
        {
            query = query.Where(s => documentNumbers.Contains(s.Number));
        }
        
        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(s => clientIds.Contains(s.ClientId));
        }
        
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => resourceIds.Contains(sr.ResourceId)));
        }
        
        if (unitIds != null && unitIds.Any())
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => unitIds.Contains(sr.UnitOfMeasureId)));
        }

        return await query
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Number)
            .ToListAsync(cancellationToken);
    }
}