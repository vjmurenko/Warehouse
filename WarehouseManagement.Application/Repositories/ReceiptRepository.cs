using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class ReceiptRepository(WarehouseDbContext context) : IReceiptRepository
{
    // Existing methods
    public async Task<bool> ExistsByNumberAsync(string number)
    {
        return await context.ReceiptDocuments.AnyAsync(r => r.Number == number);
    }

    public async Task AddAsync(ReceiptDocument document, CancellationToken token)
    {
        await context.ReceiptDocuments.AddAsync(document, token);
        await context.SaveChangesAsync(token);
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

    public async Task UpdateAsync(ReceiptDocument document, CancellationToken cancellationToken = default)
    {
        context.ReceiptDocuments.Update(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ReceiptDocument document, CancellationToken cancellationToken = default)
    {
        context.ReceiptDocuments.Remove(document);
        await context.SaveChangesAsync(cancellationToken);
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

        // Date range filtering
        if (fromDate.HasValue)
        {
            query = query.Where(r => r.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.Date <= toDate.Value);
        }

        // Document numbers filtering
        if (documentNumbers != null && documentNumbers.Any())
        {
            query = query.Where(r => documentNumbers.Contains(r.Number));
        }

        // Resource filtering
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => resourceIds.Contains(rr.ResourceId)));
        }

        // Unit filtering
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