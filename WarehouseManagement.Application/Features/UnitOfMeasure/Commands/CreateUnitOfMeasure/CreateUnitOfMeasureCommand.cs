using MediatR;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.CreateUnitOfMeasure;

public record CreateUnitOfMeasureCommand(string Name) : IRequest<Guid>;