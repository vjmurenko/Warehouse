using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetActiveUnitOfMeasures;

public class GetActiveUnitOfMeasuresQueryHandler : IRequestHandler<GetActiveUnitOfMeasuresQuery, List<UnitOfMeasureDto>>
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public GetActiveUnitOfMeasuresQueryHandler(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    public async Task<List<UnitOfMeasureDto>> Handle(GetActiveUnitOfMeasuresQuery request, CancellationToken cancellationToken)
    {
        var units = await _unitOfMeasureService.GetActiveAsync();
        return units.Select(u => new UnitOfMeasureDto(u.Id, u.Name, u.IsActive)).ToList();
    }
}