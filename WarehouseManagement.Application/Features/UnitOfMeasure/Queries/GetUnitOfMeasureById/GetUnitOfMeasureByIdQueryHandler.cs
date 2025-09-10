using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasureById;

public class GetUnitOfMeasureByIdQueryHandler : IRequestHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureDto?>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public GetUnitOfMeasureByIdQueryHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<UnitOfMeasureDto?> Handle(GetUnitOfMeasureByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _unitOfMeasureService.GetByIdAsync(request.Id);
        return unit != null ? new UnitOfMeasureDto(unit.Id, unit.Name, unit.IsActive) : null;
    }
}