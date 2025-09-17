using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class ReceiptRepository(WarehouseDbContext context) : RepositoryBase<ReceiptDocument>(context), IReceiptRepository
{
    public async Task<bool> ExistsByNumberAsync(string number)
    {
        return await context.ReceiptDocuments.AnyAsync(r => r.Number == number);
    }
    
    public async Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptDocuments
            .Include(r => r.ReceiptResources)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.ReceiptDocuments.Where(r => r.Number == number);
        
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
    
    public async Task<List<ReceiptDocument>> GetFilteredAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ReceiptDocuments
            .Include(r => r.ReceiptResources)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var fromDateUtc = fromDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
                : fromDate.Value.ToUniversalTime();
            query = query.Where(r => r.Date >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = toDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : toDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(r => r.Date <= toDateUtc);
        }

        if (documentNumbers != null && documentNumbers.Any())
        {
            query = query.Where(r => documentNumbers.Contains(r.Number));
        }
        
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => resourceIds.Contains(rr.ResourceId)));
        }
        
        if (unitIds != null && unitIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => unitIds.Contains(rr.UnitOfMeasureId)));
        }

        return await query
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Number)
            .ToListAsync(cancellationToken);
    }
}