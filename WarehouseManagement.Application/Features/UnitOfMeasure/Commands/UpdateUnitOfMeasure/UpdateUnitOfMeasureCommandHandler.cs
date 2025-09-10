using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.UpdateUnitOfMeasure;

public class UpdateUnitOfMeasureCommandHandler : IRequestHandler<UpdateUnitOfMeasureCommand, Unit>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public UpdateUnitOfMeasureCommandHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<Unit> Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfMeasureService.UpdateUnitOfMeasureAsync(request.Id, request.Name);
        return Unit.Value;
    }
}