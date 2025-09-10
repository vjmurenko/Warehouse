using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.Resources.Commands.CreateResource;
using WarehouseManagement.Application.Features.Resources.Commands.UpdateResource;
using WarehouseManagement.Application.Features.Resources.Commands.DeleteResource;
using WarehouseManagement.Application.Features.Resources.Commands.ArchiveResource;
using WarehouseManagement.Application.Features.Resources.Commands.ActivateResource;
using WarehouseManagement.Application.Features.Resources.Queries.GetResources;
using WarehouseManagement.Application.Features.Resources.Queries.GetActiveResources;
using WarehouseManagement.Application.Features.Resources.Queries.GetResourceById;
using WarehouseManagement.Application.Features.Resources.DTOs;
using WarehouseManagement.Web.Controllers.Base;

namespace WarehouseManagement.Web.Controllers;

public class ResourcesController(
    IMediator mediator) : NamedEntityControllerBase<
        ResourceDto,
        GetResourcesQuery,
        GetActiveResourcesQuery,
        GetResourceByIdQuery,
        CreateResourceCommand,
        UpdateResourceCommand,
        DeleteResourceCommand,
        ArchiveResourceCommand,
        ActivateResourceCommand>(mediator)
{
    protected override GetResourceByIdQuery CreateGetByIdQuery(Guid id) => new(id);
    protected override DeleteResourceCommand CreateDeleteCommand(Guid id) => new(id);
    protected override ArchiveResourceCommand CreateArchiveCommand(Guid id) => new(id);
    protected override ActivateResourceCommand CreateActivateCommand(Guid id) => new(id);
    protected override string GetEntityName() => "Resource";
}
