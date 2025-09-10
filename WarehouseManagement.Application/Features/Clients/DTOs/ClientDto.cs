using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive
) : INamedEntityDto;