using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.UnitOfMeasure.Commands.CreateUnitOfMeasure;
using WarehouseManagement.Application.Features.UnitOfMeasure.Commands.UpdateUnitOfMeasure;
using WarehouseManagement.Application.Features.UnitOfMeasure.Commands.DeleteUnitOfMeasure;
using WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ArchiveUnitOfMeasure;
using WarehouseManagement.Application.Features.UnitOfMeasure.Commands.ActivateUnitOfMeasure;
using WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasures;
using WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetActiveUnitOfMeasures;
using WarehouseManagement.Application.Features.UnitOfMeasure.Queries.GetUnitOfMeasureById;
using WarehouseManagement.Application.Features.UnitOfMeasure.DTOs;
using WarehouseManagement.Web.Controllers.Base;

namespace WarehouseManagement.Web.Controllers;

public class UnitOfMeasureController(
    IMediator mediator) : NamedEntityControllerBase<
        UnitOfMeasureDto,
        GetUnitOfMeasuresQuery,
        GetActiveUnitOfMeasuresQuery,
        GetUnitOfMeasureByIdQuery,
        CreateUnitOfMeasureCommand,
        UpdateUnitOfMeasureCommand,
        DeleteUnitOfMeasureCommand,
        ArchiveUnitOfMeasureCommand,
        ActivateUnitOfMeasureCommand>(mediator)
{
    protected override GetUnitOfMeasureByIdQuery CreateGetByIdQuery(Guid id) => new(id);
    protected override DeleteUnitOfMeasureCommand CreateDeleteCommand(Guid id) => new(id);
    protected override ArchiveUnitOfMeasureCommand CreateArchiveCommand(Guid id) => new(id);
    protected override ActivateUnitOfMeasureCommand CreateActivateCommand(Guid id) => new(id);
    protected override string GetEntityName() => "UnitOfMeasure";
}