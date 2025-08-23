namespace WarehouseManagement.Application.Features.BalanceQueries.DTOs;

public record BalanceDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureName,
    decimal Quantity
);
