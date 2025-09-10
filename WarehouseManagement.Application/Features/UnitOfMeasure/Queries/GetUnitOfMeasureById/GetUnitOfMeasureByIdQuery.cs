using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasureById;

public record GetUnitOfMeasureByIdQuery(Guid Id) : IRequest<UnitOfMeasureDto?>;