using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasures;

public record GetUnitOfMeasuresQuery() : IRequest<List<UnitOfMeasureDto>>;