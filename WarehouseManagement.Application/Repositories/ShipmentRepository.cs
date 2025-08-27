using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class ShipmentRepository(WarehouseDbContext context) : IShipmentRepository
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

    public async Task AddAsync(ShipmentDocument document, CancellationToken cancellationToken = default)
    {
        await context.ShipmentDocuments.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(ShipmentDocument document, CancellationToken cancellationToken = default)
    {
        context.ShipmentDocuments.Update(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ShipmentDocument document, CancellationToken cancellationToken = default)
    {
        context.ShipmentDocuments.Remove(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ShipmentDocument>> GetFilteredAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .AsQueryable();

        // Date range filtering
        if (fromDate.HasValue)
        {
            query = query.Where(s => s.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.Date <= toDate.Value);
        }

        // Document numbers filtering
        if (documentNumbers != null && documentNumbers.Any())
        {
            query = query.Where(s => documentNumbers.Contains(s.Number));
        }

        // Resource filtering
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(s => s.ShipmentResources.Any(sr => resourceIds.Contains(sr.ResourceId)));
        }

        // Unit filtering
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