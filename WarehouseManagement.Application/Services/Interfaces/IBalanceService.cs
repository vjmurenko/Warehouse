using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IBalanceService
{
     Task IncreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct);
     Task DecreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct);
}