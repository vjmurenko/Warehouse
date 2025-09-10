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
    }

    public async Task<ShipmentDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ShipmentDocuments
            .Include(s => s.ShipmentResources)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task UpdateAsync(ShipmentDocument document, CancellationToken cancellationToken = default)
    {
        context.ShipmentDocuments.Update(document);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ShipmentDocument document, CancellationToken cancellationToken = default)
    {
        context.ShipmentDocuments.Remove(document);
        return Task.CompletedTask;
    }


}