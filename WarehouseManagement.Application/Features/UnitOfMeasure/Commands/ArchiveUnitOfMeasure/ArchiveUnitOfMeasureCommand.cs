using MediatR;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ArchiveUnitOfMeasure;

public record ArchiveUnitOfMeasureCommand(Guid Id) : IRequest<Unit>;