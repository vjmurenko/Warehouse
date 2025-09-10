using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.Clients.Commands.CreateClient;
using WarehouseManagement.Application.Features.Clients.Commands.UpdateClient;
using WarehouseManagement.Application.Features.Clients.Commands.DeleteClient;
using WarehouseManagement.Application.Features.Clients.Commands.ArchiveClient;
using WarehouseManagement.Application.Features.Clients.Commands.ActivateClient;
using WarehouseManagement.Application.Features.Clients.Queries.GetClients;
using WarehouseManagement.Application.Features.Clients.Queries.GetActiveClients;
using WarehouseManagement.Application.Features.Clients.Queries.GetClientById;
using WarehouseManagement.Application.Features.Clients.DTOs;
using WarehouseManagement.Web.Controllers.Base;

namespace WarehouseManagement.Web.Controllers;

public class ClientsController(
    IMediator mediator) : NamedEntityControllerBase<
        ClientDto,
        GetClientsQuery,
        GetActiveClientsQuery,
        GetClientByIdQuery,
        CreateClientCommand,
        UpdateClientCommand,
        DeleteClientCommand,
        ArchiveClientCommand,
        ActivateClientCommand>(mediator)
{
    protected override GetClientByIdQuery CreateGetByIdQuery(Guid id) => new(id);
    protected override DeleteClientCommand CreateDeleteCommand(Guid id) => new(id);
    protected override ArchiveClientCommand CreateArchiveCommand(Guid id) => new(id);
    protected override ActivateClientCommand CreateActivateCommand(Guid id) => new(id);
    protected override string GetEntityName() => "Client";
}