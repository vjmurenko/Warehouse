using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Repositories;

public sealed class ShipmentRepository(WarehouseDbContext context) : RepositoryBase<ShipmentDocument>(context), IShipmentRepository
{
    public async Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.ShipmentDocuments.Where(s => s.Number == number);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        
        var result = await query.AnyAsync(cancellationToken);
        return result;
    }

    public async Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        
        var local = context.ShipmentDocuments.Local.SingleOrDefault(s => s.Id == id);
        if (local is not null)
        {
            return local;
        }
        
        var result = await context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        return result;
    }
}