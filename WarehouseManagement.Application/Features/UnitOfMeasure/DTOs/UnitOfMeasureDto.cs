using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;

public record UnitOfMeasureDto(
    Guid Id,
    string Name,
    bool IsActive
) : INamedEntityDto;