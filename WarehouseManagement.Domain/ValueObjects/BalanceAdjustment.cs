namespace WarehouseManagement.Domain.ValueObjects;

public record BalanceDelta(Guid ResourceId, Guid UnitOfMeasureId, decimal Quantity);