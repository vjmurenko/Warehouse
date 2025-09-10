using MediatR;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.UpdateUnitOfMeasure;

public record UpdateUnitOfMeasureCommand(
    Guid Id,
    string Name
) : IRequest<Unit>;