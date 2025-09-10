using MediatR;
using WarehouseManagement.Application.Features.Resources.DTOs;

namespace WarehouseManagement.Application.Features.Resources.Queries.GetResources;

public record GetResourcesQuery() : IRequest<List<ResourceDto>>;