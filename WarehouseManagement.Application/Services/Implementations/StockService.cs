using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Services.Implementations;

public sealed class StockService(
    IStockMovementRepository movementRepository,
    INamedEntityRepository<Resource> resourceRepository,
    INamedEntityRepository<UnitOfMeasure> unitRepository) : IStockService
{
    public async Task RecordMovements(
        Guid documentId,
        MovementType type,
        IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> items,
        CancellationToken ctx)
    {
        var movements = items
            .Where(i => i.Quantity != 0)
            .Select(i => new StockMovement(i.ResourceId, i.UnitId, i.Quantity, documentId, type))
            .ToList();

        if (movements.Count > 0)
            await movementRepository.AddRangeAsync(movements, ctx);
    }

    public async Task ReverseMovements(Guid documentId, CancellationToken ctx)
    {
        var existing = await movementRepository.GetByDocumentIdAsync(documentId, ctx);
        
        var reversals = existing
            .Select(StockMovement.CreateReversal)
            .ToList();

        if (reversals.Count > 0)
            await movementRepository.AddRangeAsync(reversals, ctx);
    }

    public async Task ValidateAvailability(
        IEnumerable<(Guid ResourceId, Guid UnitId, decimal Required)> items,
        CancellationToken ctx)
    {
        var list = items.Where(i => i.Required > 0).ToList();
        if (list.Count == 0) return;

        var keys = list.Select(i => (i.ResourceId, i.UnitId)).ToList();
        var balances = await movementRepository.GetBalancesAsync(keys, ctx);

        foreach (var (resourceId, unitId, required) in list)
        {
            var available = balances.GetValueOrDefault((resourceId, unitId), 0);
            
            if (available < required)
            {
                var resource = await resourceRepository.GetByIdAsync(resourceId, ctx);
                var unit = await unitRepository.GetByIdAsync(unitId, ctx);
                
                throw new InsufficientBalanceException(
                    resource?.Name ?? "Unknown",
                    unit?.Name ?? "Unknown",
                    required,
                    available);
            }
        }
    }
}
