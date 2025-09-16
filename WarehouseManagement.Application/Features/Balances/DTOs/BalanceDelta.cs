namespace WarehouseManagement.Application.Features.Balances.DTOs;

public record BalanceDelta(Guid ResourceId, Guid UnitOfMeasureId, decimal Quantity);