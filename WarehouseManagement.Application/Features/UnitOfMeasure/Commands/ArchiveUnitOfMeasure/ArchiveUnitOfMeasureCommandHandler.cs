using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ArchiveUnitOfMeasure;

public class ArchiveUnitOfMeasureCommandHandler : IRequestHandler<ArchiveUnitOfMeasureCommand, Unit>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public ArchiveUnitOfMeasureCommandHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<Unit> Handle(ArchiveUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfMeasureService.ArchiveAsync(request.Id);
        return Unit.Value;
    }
}