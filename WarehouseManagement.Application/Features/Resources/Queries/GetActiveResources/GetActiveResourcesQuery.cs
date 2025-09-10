using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetActiveResources;

public record GetActiveResourcesQuery() : IRequest<List<ResourceDto>>;