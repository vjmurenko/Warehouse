using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ReceiptRepository(WarehouseDbContext context) : RepositoryBase<ReceiptDocument>(context), IReceiptRepository
{
    public async Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var local = context.ReceiptDocuments.Local.SingleOrDefault(r => r.Id == id);
        if (local is not null)
        {
            return local;
        }
        
        var result = await context.ReceiptDocuments
            .Include(r => r.ReceiptResources)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        return result;
    }

    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.ReceiptDocuments.Where(r => r.Number == number);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var result = await query.AnyAsync(cancellationToken);
        if (result)
        {
            throw new InvalidOperationException($"Документ с номером {number} уже существует");
        }

        return false;
    }
}