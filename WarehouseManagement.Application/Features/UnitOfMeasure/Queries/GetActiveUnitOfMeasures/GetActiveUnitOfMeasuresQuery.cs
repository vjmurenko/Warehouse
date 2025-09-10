using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetActiveUnitOfMeasures;

public record GetActiveUnitOfMeasuresQuery() : IRequest<List<UnitOfMeasureDto>>;