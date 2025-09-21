namespace WarehouseManagement.Domain.ValueObjects;

public record BalanceAdjustment(Guid ResourceId, Guid UnitOfMeasureId, decimal Quantity);