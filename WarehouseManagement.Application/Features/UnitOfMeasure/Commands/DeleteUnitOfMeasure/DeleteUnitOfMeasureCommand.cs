using MediatR;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.DeleteUnitOfMeasure;

public record DeleteUnitOfMeasureCommand(Guid Id) : IRequest<Unit>;