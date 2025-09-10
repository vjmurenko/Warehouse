using MediatR;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ActivateUnitOfMeasure;

public record ActivateUnitOfMeasureCommand(Guid Id) : IRequest<Unit>;