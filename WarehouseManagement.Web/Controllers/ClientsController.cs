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
    /// <returns>List of all clients</returns>
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetClients()
    {
        var clients = await clientService.GetAllAsync();
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
    /// <returns>List of active clients</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ClientDto>>> GetActiveClients()
    {
        var clients = await clientService.GetActiveAsync();
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
    /// <returns>Client information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClientById(Guid id)
    {
        var client = await clientService.GetByIdAsync(id);
        
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
    /// <returns>Created client DTO</returns>
    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequest request)
    {
        var clientId = await clientService.CreateClientAsync(request.Name, request.Address);
        var client = await clientService.GetByIdAsync(clientId);
        
        if (client == null)
        {
            return BadRequest("Failed to create client");
        }
        
        var clientDto = new ClientDto(
            client.Id,
            client.Name,
            client.Address.Name,
            client.IsActive
        );
        
        return CreatedAtAction(nameof(GetClientById), new { id = clientId }, clientDto);
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <param name="request">Client update data</param>
    /// <returns>Updated client DTO</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
    {
        var success = await clientService.UpdateClientAsync(id, request.Name, request.Address);
        
        if (!success)
        {
            return NotFound();
        }
        
        var client = await clientService.GetByIdAsync(id);
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
    /// Delete a client
    /// </summary>
    /// <param name="id">The client ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteClient(Guid id)
    {
        var success = await clientService.DeleteAsync(id);
        
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
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveClient(Guid id)
    {
        var success = await clientService.ArchiveAsync(id);
        
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
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateClient(Guid id)
    {
        var success = await clientService.ActivateAsync(id);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}