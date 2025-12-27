using WarehouseManagement.Domain.Enums;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IStockService
{
    Task RecordMovements(Guid documentId, MovementType type, 
        IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> items, CancellationToken ctx);
    
    Task ReverseMovements(Guid documentId, CancellationToken ctx);
    
    Task ValidateAvailability(IEnumerable<(Guid ResourceId, Guid UnitId, decimal Required)> items, CancellationToken ctx);
}
