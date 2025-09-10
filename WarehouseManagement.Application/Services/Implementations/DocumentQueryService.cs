using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Services.Implementations;

/// <summary>
/// Service responsible for complex document querying and filtering logic.
/// Extracted from repositories to maintain Single Responsibility Principle.
/// Handles all complex query building logic for documents and balances.
/// </summary>
public class DocumentQueryService(WarehouseDbContext context, IReceiptRepository receiptRepository) : IDocumentQueryService
{
    public async Task<List<ReceiptDocument>> GetFilteredReceiptsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default)
    {
        return await receiptRepository.GetFilteredAsync(
            fromDate, 
            toDate, 
            documentNumbers, 
            resourceIds, 
            unitIds, 
            cancellationToken);
    }

    public async Task<List<ShipmentDocument>> GetFilteredShipmentsAsync(
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

    public async Task<List<Balance>> GetFilteredBalancesAsync(
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Balances.AsQueryable();

        // Filter out zero balances
        query = query.Where(b => b.Quantity.Value > 0);

        // Resource filtering
        if (resourceIds != null && resourceIds.Any())
        {
            query = query.Where(b => resourceIds.Contains(b.ResourceId));
        }

        // Unit filtering  
        if (unitIds != null && unitIds.Any())
        {
            query = query.Where(b => unitIds.Contains(b.UnitOfMeasureId));
        }

        return await query
            .OrderBy(b => b.ResourceId)
            .ThenBy(b => b.UnitOfMeasureId)
            .ToListAsync(cancellationToken);
    }
}