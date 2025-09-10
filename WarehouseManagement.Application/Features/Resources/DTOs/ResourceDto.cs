using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Application.Features.Resources.DTOs;

public record ResourceDto(
    Guid Id,
    string Name,
    bool IsActive
) : INamedEntityDto;