using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ActivateUnitOfMeasure;

public class ActivateUnitOfMeasureCommandHandler : IRequestHandler<ActivateUnitOfMeasureCommand, Unit>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public ActivateUnitOfMeasureCommandHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<Unit> Handle(ActivateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfMeasureService.ActivateAsync(request.Id);
        return Unit.Value;
    }
}