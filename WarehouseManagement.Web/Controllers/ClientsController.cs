using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Dtos.Client;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    /// <summary>
    /// Get all clients
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of all clients</returns>
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetClients(CancellationToken ctx)
    {
        var clients = await clientService.GetAllAsync(ctx);
        var clientDtos = clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Address.Name,
            c.IsActive
        )).ToList();
        
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
        var clients = await clientService.GetActiveAsync(ctx);
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
        var client = await clientService.GetByIdAsync(id, ctx);
        
        if (client == null)
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
        var clientId = await clientService.CreateClientAsync(request.Name, request.Address, ctx);
        return CreatedAtAction(nameof(GetClientById), new { id = clientId }, clientId);
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
        var success = await clientService.UpdateClientAsync(id, request.Name, request.Address, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
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
        var success = await clientService.DeleteAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
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
        var success = await clientService.ArchiveAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
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
        var success = await clientService.ActivateAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}