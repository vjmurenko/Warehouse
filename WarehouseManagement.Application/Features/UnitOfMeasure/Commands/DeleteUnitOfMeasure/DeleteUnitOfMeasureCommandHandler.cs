using MediatR;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.DeleteUnitOfMeasure;

public class DeleteUnitOfMeasureCommandHandler : IRequestHandler<DeleteUnitOfMeasureCommand, Unit>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public DeleteUnitOfMeasureCommandHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<Unit> Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfMeasureService.DeleteAsync(request.Id);
        return Unit.Value;
    }
}