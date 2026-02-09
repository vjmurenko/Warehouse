namespace WarehouseManagement.Application.Services.Interfaces;

public interface IBalanceService
{
    Task UpdateBalances(IEnumerable<(Guid ResourceId, Guid UnitId, decimal Quantity)> items, CancellationToken ctx);
    Task ValidateAvailability(IEnumerable<(Guid ResourceId, Guid UnitId, decimal Required)> items, CancellationToken ctx);
}
