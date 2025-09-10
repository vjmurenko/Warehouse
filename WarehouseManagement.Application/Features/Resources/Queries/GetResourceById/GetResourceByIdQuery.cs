using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetResourceById;

public record GetResourceByIdQuery(Guid Id) : IRequest<ResourceDto?>;