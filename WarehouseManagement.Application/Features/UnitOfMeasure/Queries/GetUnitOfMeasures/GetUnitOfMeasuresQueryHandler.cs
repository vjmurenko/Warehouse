using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasures;

public class GetUnitOfMeasuresQueryHandler : IRequestHandler<GetUnitOfMeasuresQuery, List<UnitOfMeasureDto>>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public GetUnitOfMeasuresQueryHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<List<UnitOfMeasureDto>> Handle(GetUnitOfMeasuresQuery request, CancellationToken cancellationToken)
    {
        var units = await _unitOfMeasureService.GetAllAsync();
        return units.Select(u => new UnitOfMeasureDto(u.Id, u.Name, u.IsActive)).ToList();
    }
}