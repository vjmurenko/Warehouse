using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Dtos.Client;
using WarehouseManagement.Application.Features.References.Commands;
using WarehouseManagement.Application.Features.References.Commands.Create.CreateClient;
using WarehouseManagement.Application.Features.References.Commands.Delete;
using WarehouseManagement.Application.Features.References.Commands.Update.UpdateClient;
using WarehouseManagement.Application.Features.References.Queries;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClientsController(IMediator mediator, ILogger<ClientsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all clients
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of all clients</returns>
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetClients(CancellationToken ctx)
    {
        logger.LogInformation("Getting all clients");
        var clients = await mediator.Send(new GetAllReferencesQuery<Client>(), ctx);
        var clientDtos = clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Address.Name,
            c.IsActive
        )).ToList();

        logger.LogInformation("Successfully retrieved {Count} clients", clientDtos.Count);
        return Ok(clientDtos);
    }

    /// <summary>
    /// Get active clients only
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of active clients</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ClientDto>>> GetActiveClients(CancellationToken ctx)
    {
        var clients = await mediator.Send(new GetActiveReferencesQuery<Client>(), ctx);
        var clientDtos = clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Address.Name,
            c.IsActive
        )).ToList();

        return Ok(clientDtos);
    }

    /// <summary>
    /// Get a specific client by ID
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>Client information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClientById(Guid id, CancellationToken ctx)
    {
        var client = await mediator.Send(new GetReferenceByIdQuery<Client>(id), ctx);

        if (client is null)
        {
            return NotFound();
        }

        var clientDto = new ClientDto(
            client.Id,
            client.Name,
            client.Address.Name,
            client.IsActive
        );

        return Ok(clientDto);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    /// <param name="request">Client creation data</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>ID of the created client</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateClient([FromBody] CreateClientRequest request, CancellationToken ctx)
    {
        logger.LogInformation("Creating new client with name: {ClientName}", request.Name);
        
        var clientId = await mediator.Send(new CreateClientCommand(request.Name, request.Address), ctx);
        
        logger.LogInformation("Successfully created client with ID: {ClientId}", clientId);
        return CreatedAtAction(nameof(GetClientById), new {id = clientId}, clientId);
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="request">Client update data</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ctx)
    {
        await mediator.Send(new UpdateClientCommand(id, request.Name, request.Address), ctx);

        return NoContent();
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteClient(Guid id, CancellationToken ctx)
    {
        logger.LogInformation("Deleting client with ID: {ClientId}", id);

        await mediator.Send(new DeleteReferenceCommand<Client>(id), ctx);

        logger.LogInformation("Successfully deleted client with ID: {ClientId}", id);
        return NoContent();
    }

    /// <summary>
    /// Archive a client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveClient(Guid id, CancellationToken ctx)
    {
        await mediator.Send(new ArchiveReferenceCommand<Client>(id), ctx);

        return NoContent();
    }

    /// <summary>
    /// Activate a client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateClient(Guid id, CancellationToken ctx)
    {
        await mediator.Send(new ActivateReferenceCommand<Client>(id), ctx);
        
        return NoContent();
    }
}