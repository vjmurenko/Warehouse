using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ReceiptRepository(WarehouseDbContext context, ILogger<ReceiptRepository> logger) : RepositoryBase<ReceiptDocument>(context), IReceiptRepository
{
    public async Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting receipt document with ID {Id} including resources", id);
        
        var local = context.ReceiptDocuments.Local.SingleOrDefault(r => r.Id == id);
        if (local is not null)
        {
            logger.LogInformation("Receipt document with ID {Id} found in local cache", id);
            return local;
        }
        
        var result = await context.ReceiptDocuments
            .Include(r => r.ReceiptResources)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        logger.LogInformation("Receipt document with ID {Id} found: {Found}", id, result is not null);
        return result;
    }

    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if receipt document with number {Number} exists, excluding ID {ExcludeId}", number, excludeId);
        var query = context.ReceiptDocuments.Where(r => r.Number == number);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var result = await query.AnyAsync(cancellationToken);
        if (result)
        {
            logger.LogInformation("Receipt document with number {Number} exists: {Exists}", number, result);
            throw new InvalidOperationException($"Документ с номером {number} уже существует");
        }

        return false;
    }

    public async Task<List<ReceiptDocument>> GetFilteredAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting filtered receipt documents with parameters - FromDate: {FromDate}, ToDate: {ToDate}, DocumentNumbers: {DocumentNumbersCount}, Resources: {ResourceIdsCount}, Units: {UnitIdsCount}",
            fromDate, toDate, documentNumbers?.Count ?? 0, resourceIds?.Count ?? 0, unitIds?.Count ?? 0);
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

        if (documentNumbers is not null && documentNumbers.Any())
        {
            query = query.Where(r => documentNumbers.Contains(r.Number));
        }

        if (resourceIds is not null && resourceIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => resourceIds.Contains(rr.ResourceId)));
        }

        if (unitIds is not null && unitIds.Any())
        {
            query = query.Where(r => r.ReceiptResources.Any(rr => unitIds.Contains(rr.UnitOfMeasureId)));
        }

        var result = await query
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Number)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {Count} receipt documents", result.Count);
        return result;
    }
}