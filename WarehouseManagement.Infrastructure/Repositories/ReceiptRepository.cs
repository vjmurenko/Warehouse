using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
}